using UnityEngine;
using System.Collections.Generic; // HashSetを使用するために必要

[RequireComponent(typeof(Collider))] // このスクリプトにはColliderが必須
public class SphereWalker : MonoBehaviour
{
    [Header("Main Sphere Configuration")]
    [Tooltip("移動の基準となるメインの球体。")]
    public Transform mainSphere;

    [Header("Movement Settings")]
    [Tooltip("オブジェクトの移動速度。")]
    public float moveSpeed = 5.0f;
    [Tooltip("オブジェクトが移動方向を向く際の回転速度。")]
    public float rotationSpeed = 10.0f;
    [Tooltip("メイン球体の表面からオブジェクトの基点（ピボット）までの基本的な高さオフセット。")]
    public float baseObjectHeightOffset = 0.5f;

    [Header("Sub-Sphere Interaction")]
    [Tooltip("上昇効果を発生させるサブ球体に設定するタグ名。")]
    public string subSphereTag = "SubSphere";

    // メイン球体のワールド空間での中心と、スケーリングを考慮した有効半径
    private Vector3 _mainSphereWorldCenter;
    private float _mainSphereEffectiveRadius;

    // サブ球体によって追加される高さ
    private float _additionalHeightFromSubSpheres = 0f;

    // 現在接触している（トリガー範囲内にある）サブ球体のColliderを保持するセット
    private HashSet<Collider> _contactingSubSpheres;

    private Camera _mainCamera; // 移動方向の計算に使用

    void Start()
    {
        // 自身のColliderをトリガーとして設定
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("SphereWalkerWithElevation: このGameObjectにColliderがアタッチされていません！", this);
            enabled = false;
            return;
        }

        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("SphereWalkerWithElevation: メインカメラが見つかりません。", this);
            enabled = false;
            return;
        }

        if (mainSphere == null)
        {
            Debug.LogError("SphereWalkerWithElevation: Main Sphereがアタッチされていません！", this);
            enabled = false;
            return;
        }
        UpdateMainSphereDetails(); // メイン球体の情報を初期化

        _contactingSubSpheres = new HashSet<Collider>();
    }

    /// <summary>
    /// メイン球体の詳細（ワールド中心、有効半径）を更新します。
    /// </summary>
    void UpdateMainSphereDetails()
    {
        SphereCollider sc = mainSphere.GetComponent<SphereCollider>();
        if (sc == null)
        {
            Debug.LogErrorFormat(this, "SphereWalkerWithElevation: メイン球体 '{0}' にSphereColliderがアタッチされていません！", mainSphere.name);
            enabled = false;
            return;
        }
        _mainSphereWorldCenter = mainSphere.TransformPoint(sc.center);
        Vector3 sphereScale = mainSphere.lossyScale;
        float maxScaleComponent = Mathf.Max(Mathf.Abs(sphereScale.x), Mathf.Abs(sphereScale.y), Mathf.Abs(sphereScale.z));
        _mainSphereEffectiveRadius = sc.radius * maxScaleComponent;
    }

    /// <summary>
    /// オブジェクトをメイン球体の表面（＋追加高さ）に正確に配置し、向きを調整します。
    /// </summary>
    void SnapToCurrentSurface()
    {
        if (mainSphere == null) return;

        Vector3 directionFromCenter = (transform.position - _mainSphereWorldCenter).normalized;
        if (directionFromCenter == Vector3.zero) // オブジェクトが球中心にある場合
        {
            directionFromCenter = transform.up != Vector3.zero ? transform.up : Vector3.up; // 安全なデフォルト方向
        }

        float totalHeightOffset = _mainSphereEffectiveRadius + baseObjectHeightOffset + _additionalHeightFromSubSpheres;
        transform.position = _mainSphereWorldCenter + directionFromCenter * totalHeightOffset;
        transform.up = directionFromCenter;
    }

    void Update()
    {
        if (mainSphere == null || _mainCamera == null) return;

        // 入力処理
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // カメラ基準の移動方向計算
        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 desiredMoveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        // 移動と表面追従
        if (desiredMoveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 surfaceNormal = (transform.position - _mainSphereWorldCenter).normalized;
            if (surfaceNormal == Vector3.zero) surfaceNormal = transform.up;

            Vector3 moveOnSphereTangent = Vector3.ProjectOnPlane(desiredMoveDirection, surfaceNormal).normalized;
            Vector3 newPosition = transform.position + moveOnSphereTangent * moveSpeed * Time.deltaTime;

            Vector3 directionToNewPos = (newPosition - _mainSphereWorldCenter).normalized;
            float totalHeightOffset = _mainSphereEffectiveRadius + baseObjectHeightOffset + _additionalHeightFromSubSpheres;
            transform.position = _mainSphereWorldCenter + directionToNewPos * totalHeightOffset;
            transform.up = directionToNewPos;

            if (moveOnSphereTangent.sqrMagnitude > 0.01f)
            {
                Quaternion targetLookRotation = Quaternion.LookRotation(moveOnSphereTangent, directionToNewPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            SnapToCurrentSurface(); // 入力がない場合は現在の位置で表面にスナップ
        }
    }

    void FixedUpdate()
    {
        // 現在接触しているサブ球体の中から、最大の追加高さを計算
        float maxHeightFromSubSpheres = 0f;
        if (_contactingSubSpheres.Count > 0)
        {
            List<Collider> collidersToRemove = new List<Collider>(); // 無効になったコライダーを記録

            foreach (Collider subCollider in _contactingSubSpheres)
            {
                if (subCollider == null || !subCollider.gameObject.activeInHierarchy)
                {
                    collidersToRemove.Add(subCollider); // 後でHashSetから削除
                    continue;
                }

                SphereCollider sc = subCollider.GetComponent<SphereCollider>();
                if (sc != null)
                {
                    Vector3 subSphereWorldCenter = subCollider.transform.TransformPoint(sc.center);
                    Vector3 subScale = subCollider.transform.lossyScale;
                    float subMaxScale = Mathf.Max(Mathf.Abs(subScale.x), Mathf.Abs(subScale.y), Mathf.Abs(subScale.z));
                    float subEffectiveRadius = sc.radius * subMaxScale;

                    // サブ球体の中心から、メイン球体の法線(evalNormalFromMainSphere)方向に
                    // サブ球体の半径分進んだ点を、その法線方向におけるサブ球体の「頂点」とみなす。
                    Vector3 subSphereTopPointInEvalNormal = subSphereWorldCenter + ((transform.position - _mainSphereWorldCenter).normalized * subEffectiveRadius);

                    // メイン球体の中心から、そのサブ球体の「頂点」までの距離
                    float distanceToSubSphereTopFromMainCenter = Vector3.Distance(_mainSphereWorldCenter, subSphereTopPointInEvalNormal);

                    // メイン球体の表面からサブ球体の「頂点」までの符号付き高さ。
                    // これが実質的な「隆起の高さ」または「食い込みの深さ（の逆）」を示す。
                    float elevationProvidedBySubSphere = distanceToSubSphereTopFromMainCenter - _mainSphereEffectiveRadius;

                    // 実際にオブジェクトを持ち上げる追加高さは0以上とし、現在の最大値と比較。


                    float maxCalculatedAdditionalHeight = Mathf.Max(0f, elevationProvidedBySubSphere);
                     
                    
                }
            }
            // 無効なコライダーをHashSetから削除
            foreach(var colToRemove in collidersToRemove)
            {
                _contactingSubSpheres.Remove(colToRemove);
            }
        }
        _additionalHeightFromSubSpheres = maxHeightFromSubSpheres;
    }

    // --- サブ球体との接触処理 ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(subSphereTag))
        {
            // Debug.Log($"Entered SubSphere: {other.name}");
            _contactingSubSpheres.Add(other);
            // FixedUpdateで高さが再計算される
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Enterで追加されているはずだが、念のためStayでも追加/存在確認
        if (other.CompareTag(subSphereTag))
        {
            if (!_contactingSubSpheres.Contains(other)) // まれなケースに対応
            {
                _contactingSubSpheres.Add(other);
            }
            // FixedUpdateで高さが再計算される
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(subSphereTag))
        {
            // Debug.Log($"Exited SubSphere: {other.name}");
            _contactingSubSpheres.Remove(other);
            // FixedUpdateで高さが再計算される (これが最後のサブ球体なら0になる)
        }
    }
}