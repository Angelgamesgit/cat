using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タッチ位置に対して、シーン上に存在するSpot Lightを照射するシステム。
///
/// Spot Lightはこのスクリプトが生成しない。
/// 必ずシーン上に配置したLightをInspectorから指定する。
///
/// タッチした場所に対象オブジェクトが存在する場合は、
/// その対象オブジェクトをGameSystemの目的地として使用する。
///
/// 対象が存在しない場合は、GameSystem側で通常通り
/// タッチ地点を目的地として使用する。
/// </summary>
public class LightTargetSystem : MonoBehaviour
{
    [Header("Camera")]

    [Tooltip("タッチ判定に使用するカメラ")]
    [SerializeField]
    private Camera targetCamera;


    [Header("Scene Spot Light")]

    [Tooltip("シーン上に配置してあるSpot Light")]
    [SerializeField]
    private Light sceneSpotLight;

    [Tooltip("Spot Lightをタッチ方向へ向ける")]
    [SerializeField]
    private bool rotateSpotLight = true;


    [Header("Spot Light Settings")]

    [Tooltip("Spot Lightの照射距離")]
    [SerializeField]
    private float lightRange = 30f;

    [Tooltip("Spot Lightの照射角度")]
    [SerializeField]
    [Range(1f, 179f)]
    private float lightAngle = 30f;


    [Header("Target Detection")]

    [Tooltip("光が当たった対象として扱うLayer")]
    [SerializeField]
    private LayerMask targetLayer;

    [Tooltip("同時に検出するColliderの最大数")]
    [SerializeField]
    private int maxTargetCount = 64;


    [Header("Target Selection")]

    [Tooltip("複数の対象がある場合、タッチ地点に最も近いものを選択")]
    [SerializeField]
    private bool selectNearestTarget = true;


    [Header("Debug")]

    [SerializeField]
    private bool debugLog = false;

    [SerializeField]
    private bool drawDebug = true;


    private Collider[] colliderBuffer;


    /// <summary>
    /// 最後にタッチしたワールド座標
    /// </summary>
    public Vector3 LastTouchPosition
    {
        get;
        private set;
    }


    /// <summary>
    /// 現在選択されている対象。
    /// 対象が存在しない場合はnull。
    /// </summary>
    public Transform CurrentTarget
    {
        get;
        private set;
    }


    private void Awake()
    {
        colliderBuffer =
            new Collider[maxTargetCount];

        SetupLight();
    }


    /// <summary>
    /// シーン上のSpot Lightを設定する。
    /// </summary>
    private void SetupLight()
    {
        if (sceneSpotLight == null)
        {
            Debug.LogError(
                "[LightTargetSystem] " +
                "Scene Spot Lightが設定されていません。"
            );

            return;
        }

        if (sceneSpotLight.type != LightType.Spot)
        {
            Debug.LogWarning(
                "[LightTargetSystem] " +
                "指定されたLightがSpot Lightではありません。"
            );
        }

        sceneSpotLight.range =
            lightRange;

        sceneSpotLight.spotAngle =
            lightAngle;
    }


    /// <summary>
    /// GameSystemから呼び出す。
    ///
    /// screenPosition:
    /// PCならInput.mousePosition
    /// スマホならInput.touch.position
    ///
    /// 戻り値:
    /// 光の範囲内に対象があれば、そのTransform。
    /// なければnull。
    /// </summary>
    public Transform ProcessTouch(
        Vector2 screenPosition)
    {
        if (targetCamera == null)
        {
            Debug.LogError(
                "[LightTargetSystem] " +
                "Target Cameraが設定されていません。"
            );

            return null;
        }

        if (sceneSpotLight == null)
        {
            Debug.LogError(
                "[LightTargetSystem] " +
                "Scene Spot Lightが設定されていません。"
            );

            return null;
        }


        Ray ray =
            targetCamera.ScreenPointToRay(
                screenPosition
            );


        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                lightRange
            );


        if (hits == null ||
            hits.Length == 0)
        {
            return null;
        }


        // カメラから近い順に並べる
        System.Array.Sort(
            hits,
            (a, b) =>
                a.distance.CompareTo(
                    b.distance
                )
        );


        // 最初に見つかった
        // 球体表面などのColliderをタッチ地点として使用
        RaycastHit surfaceHit =
            hits[0];


        Vector3 touchPosition =
            surfaceHit.point;


        LastTouchPosition =
            touchPosition;


        // Spot Lightをタッチ方向へ向ける
        AimSpotLight(
            touchPosition
        );


        // 光の範囲にある対象を探す
        Transform target =
            FindTargetInSpotLight(
                touchPosition
            );


        CurrentTarget =
            target;


        if (debugLog)
        {
            if (target != null)
            {
                Debug.Log(
                    "[LightTargetSystem] " +
                    "光が当たった対象: " +
                    target.name
                );
            }
            else
            {
                Debug.Log(
                    "[LightTargetSystem] " +
                    "光が当たった対象なし"
                );
            }
        }


        return target;
    }


    /// <summary>
    /// Spot Lightをカメラから
    /// タッチ地点方向へ向ける。
    /// </summary>
    private void AimSpotLight(
        Vector3 targetPosition)
    {
        if (!rotateSpotLight)
        {
            return;
        }


        Transform lightTransform =
            sceneSpotLight.transform;


        // Spot Lightの位置はシーン上の位置を維持。
        // 位置をカメラに移動させない。
        Vector3 direction =
            targetPosition -
            lightTransform.position;


        if (direction.sqrMagnitude <
            0.0001f)
        {
            return;
        }


        lightTransform.rotation =
            Quaternion.LookRotation(
                direction.normalized
            );
    }


    /// <summary>
    /// Spot Lightの照射範囲内から
    /// 目的地候補を検索。
    /// </summary>
    private Transform FindTargetInSpotLight(
        Vector3 touchPosition)
    {
        int hitCount =
            Physics.OverlapSphereNonAlloc(
                touchPosition,
                lightRange,
                colliderBuffer,
                targetLayer,
                QueryTriggerInteraction.Ignore
            );


        if (hitCount <= 0)
        {
            return null;
        }


        List<Collider> candidates =
            new List<Collider>();


        for (
            int i = 0;
            i < hitCount;
            i++
        )
        {
            Collider collider =
                colliderBuffer[i];


            if (collider == null)
            {
                continue;
            }


            if (!IsInsideSpotLight(
                    collider))
            {
                continue;
            }


            if (!candidates.Contains(
                    collider))
            {
                candidates.Add(
                    collider
                );
            }
        }


        if (candidates.Count == 0)
        {
            return null;
        }


        // 1つだけならそのまま
        if (candidates.Count == 1)
        {
            return candidates[0].transform;
        }


        // 複数の場合
        if (selectNearestTarget)
        {
            return FindNearestTarget(
                candidates,
                touchPosition
            );
        }


        return candidates[0].transform;
    }


    /// <summary>
    /// ColliderがSpot Lightの円錐内に
    /// 存在するか確認する。
    /// </summary>
    private bool IsInsideSpotLight(
        Collider collider)
    {
        Vector3 targetPosition =
            collider.bounds.center;


        Vector3 direction =
            targetPosition -
            sceneSpotLight.transform.position;


        float distance =
            direction.magnitude;


        // 光の射程外
        if (distance >
            sceneSpotLight.range)
        {
            return false;
        }


        if (distance <=
            0.001f)
        {
            return true;
        }


        direction.Normalize();


        float angle =
            Vector3.Angle(
                sceneSpotLight.transform.forward,
                direction
            );


        return angle <=
            sceneSpotLight.spotAngle * 0.5f;
    }


    /// <summary>
    /// タッチ地点に最も近い対象を選択。
    /// </summary>
    private Transform FindNearestTarget(
        List<Collider> candidates,
        Vector3 touchPosition)
    {
        Collider nearest =
            null;

        float nearestDistance =
            float.MaxValue;


        foreach (
            Collider collider
            in candidates)
        {
            if (collider == null)
            {
                continue;
            }


            float distance =
                Vector3.Distance(
                    touchPosition,
                    collider.bounds.center
                );


            if (distance <
                nearestDistance)
            {
                nearestDistance =
                    distance;

                nearest =
                    collider;
            }
        }


        if (nearest == null)
        {
            return null;
        }


        return nearest.transform;
    }


    /// <summary>
    /// 光を消す。
    /// </summary>
    public void HideLight()
    {
        if (sceneSpotLight != null)
        {
            sceneSpotLight.enabled =
                false;
        }
    }


    /// <summary>
    /// 光を表示する。
    /// </summary>
    public void ShowLight()
    {
        if (sceneSpotLight != null)
        {
            sceneSpotLight.enabled =
                true;
        }
    }


    /// <summary>
    /// 現在使用しているSpot Lightを取得。
    /// </summary>
    public Light GetSpotLight()
    {
        return sceneSpotLight;
    }


    private void OnDrawGizmosSelected()
    {
        if (!drawDebug ||
            sceneSpotLight == null)
        {
            return;
        }


        Gizmos.color =
            Color.yellow;


        Gizmos.DrawRay(
            sceneSpotLight.transform.position,
            sceneSpotLight.transform.forward *
            sceneSpotLight.range
        );
    }
}