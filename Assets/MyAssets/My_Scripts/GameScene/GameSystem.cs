using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;


//  UI判定に必要
public class GameSystem : MonoBehaviour
{
    public bool isPlaying;
    public PlayerData playerData;

    [SerializeField]
    public GameObject sphere;


    public Camera mainCamera;

    // 初期カメラ位置と回転を保存する変数
    Vector3 cameraPos;
    Quaternion cameraRotation;

    [Header("Camera Settings")]
    [Tooltip("猫の正面にカメラを配置する際の距離")]
    [SerializeField] private float cameraFrontDistance;
    [Tooltip("猫の正面にカメラを配置する際の高さ")]
    [SerializeField] private float cameraFrontHeight;


    //見つける猫のオブジェクト
    [SerializeField]
    GameObject findCatPrefab;
    public GameObject findCatObject;

    CatPlayer catSystem;


    [Tooltip("このオブジェクトが正面に来るように”星”が回転する このオブジェクトとの中間にあるオブジェクトを半透明にする")]
    public GameObject targetObject;

    [Tooltip("カメラとターゲットの間にあるオブジェクトに適用するマテリアル")]
    public Material replacementMaterial;

    [Tooltip("透明度を変更するレイヤー (指定しない場合は全てのレイヤーをチェック)")]
    // -1 は全てのレイヤー
    LayerMask layerMask;

    private Dictionary<Renderer, Material[]> originalMaterials;
    private HashSet<Renderer> currentlyObstructedRenderers;

    [Tooltip("球体の表面からのオフセット (矢印が球体に埋まらないように)")]
    public float arrowOffset = 0.1f;

    SphereCollider sphereCollider;

    [SerializeField]
    FindCat findCat;
    public CatData findCatData;
public CatData[]  catData;
    public enum MissionState
    {
        None,
        Play,
    }
public static MissionState missionState = MissionState.None;

    //猫が追いかけるターゲットのオブジェクトが指定の位置にあるかをチェック　タッチで場所を変えた場合は猫とターゲットの最低距離をなくす
    // ▼▼▼ 変更点: targetObjMoveフラグは新しいロジックでは不要となるためコメントアウト ▼▼▼
    // public bool targetObjMove;
    // ▲▲▲ 変更点終了 ▲▲▲
    [SerializeField]
    DaySphereSystem daySphereSystem;
    [SerializeField]
    WeatherSystem weatherSystem;
    public GameObject kitchenObject;
    [SerializeField]
    GameObject gateObject;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        AssignVariables();
        targetObject.SetActive(false);
        // cameraPosとcameraRotationはSphereSetで初期化される
        playerData.Load();
        SphereSet(playerData.currentSphereSpec);
        //星の頂点にターゲットを配置する
        Vector3 targetPositionOnSurface = sphere.transform.position + (sphere.transform.up * sphereCollider.radius * sphere.transform.localScale.y);
        targetObject.transform.position = targetPositionOnSurface;
        targetObject.transform.up = (targetObject.transform.position - sphere.transform.position).normalized;
        kitchenObject = GameObject.FindGameObjectWithTag("Kitchen");
        // UIの初期化
    }

    void AssignVariables()
    {
        isPlaying = false;
        layerMask = 1 << LayerMask.NameToLayer("Object"); ; // -1 は全てのレイヤー
        originalMaterials = new Dictionary<Renderer, Material[]>();
        currentlyObstructedRenderers = new HashSet<Renderer>();
    }

//ボタンから呼び出される　ゲームを開始する
    public void GameStart()
    {
        targetObject.SetActive(true);
        isPlaying = true;
        sphereCollider = sphere.GetComponent<SphereCollider>();
        StartCoroutine(FindCatSet());
    }
    public IEnumerator FindCatSet()
    {
        for(int i = 0; i < catData.Length; i++)
        {
        float sphereRadius = sphereCollider.radius
         * Mathf.Max(sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z); // ローカルスケールを考慮
        Debug.Log("sphereRadius is " + sphereRadius);
        // 球体の中心からのランダムな方向ベクトルを生成
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        // 球体表面上のランダムな位置を計算 (localScaleを考慮)
        Vector3 spawnPosition = sphere.transform.position + (randomDirection * sphereRadius);
        // 球体中心から生成位置への方向を計算
        Vector3 upDirection = randomDirection.normalized;

        findCatObject = Instantiate(findCatPrefab, spawnPosition, Quaternion.identity, sphere.transform);
        findCat = findCatObject.AddComponent<FindCat>();
        findCat.StartSet(this);
        findCat.catData = catData[i];
        findCatObject.transform.up = upDirection;
        /// <summary>
        /// 後々データから反映するもの、　スフィアのデータから猫のデータを取得して、猫の種類や見た目を変える　猫のデータから、猫の行動パターンやアニメーションを変える
        /// </summary>
        yield return null; // 猫を順番に生成するための待機時間
        }
        cameraRotation = mainCamera.transform.localRotation;
    }

    /// <summary>
    /// カメラの位置を切り替える
    /// </summary>
    /// <param name="up">true: 猫の真上（初期位置）へ, false: 猫の正面へ</param>
    /// <param name="time">移動にかかる時間</param>
    public void cameraPosChange(bool up, float time)
    {
        if (catSystem == null)
        {
            Debug.LogError("CatPlayer(catSystem)が見つからないため、カメラを移動できません。");
            return;
        }

        if (up)
        {
            // 初期位置（猫の真上）にカメラを移動
            mainCamera.transform.DOMove(cameraPos, time);
            mainCamera.transform.DORotate(cameraRotation.eulerAngles, time);
            Debug.Log("Move Camera to Up Position");
        }
        else
        {
            // 猫の正面にカメラを移動
            Vector3 catPosition = catSystem.transform.position;
            Vector3 catUp = catSystem.transform.up;
            Vector3 catForward = catSystem.transform.forward;

            // 正面の位置を計算
            Vector3 frontPos = catPosition - (catForward * cameraFrontDistance) + (catUp * cameraFrontHeight);
            // カメラの回転を計算 (猫の方向を向くように)
            Vector3 directionToCat = (catPosition - frontPos).normalized;
            Quaternion frontRotation = Quaternion.LookRotation(directionToCat, catUp);
            mainCamera.transform.DOMove(frontPos, time);
            mainCamera.transform.DORotate(frontRotation.eulerAngles, time);
        }
    }

    void Update()
    {
        if (!isPlaying) return;
        GameEnd();
        BetweenCameraAndObject();
        if(Input.GetMouseButton(0))
        {
            TouchSystem();
        }
        if(Input.GetMouseButtonUp(0))
        {
            targetObject.SetActive(false);
            RotateSphereToFaceTarget();
        }

    }


    //オブジェクトとカメラの間にあるオブジェクトを専用のマテリアル（半透明）に置き換える　
    //オブジェクトとカメラの間、およびカメラと衝突しているオブジェクトを半透明にする
    void BetweenCameraAndObject()
    {
        if (targetObject == null || replacementMaterial == null || mainCamera == null)
        {
            // 必要なオブジェクトが設定されていない場合は何もしない
            return;
        }

        // 1. 今フレームで遮蔽物となるRendererのリストを新規作成
        var obstructedRenderersThisFrame = new HashSet<Renderer>();
        var cameraPosition = mainCamera.transform.position;

        // 1a. カメラとターゲットの間にあるオブジェクトをリストに追加
        var targetPosition = targetObject.transform.position;
        var directionToTarget = targetPosition - cameraPosition;
        foreach (var hit in Physics.RaycastAll(cameraPosition, directionToTarget.normalized, directionToTarget.magnitude, layerMask))
        {
            AddAllRenderersFrom(hit.collider.gameObject, obstructedRenderersThisFrame);
        }

        // 1b. カメラと猫の間にあるオブジェクトをリストに追加
        if (catSystem != null)
        {
            var catPosition = catSystem.transform.position;
            var directionToCat = catPosition - cameraPosition;
            foreach (var hit in Physics.RaycastAll(cameraPosition, directionToCat.normalized, directionToCat.magnitude, layerMask))
            {
                AddAllRenderersFrom(hit.collider.gameObject, obstructedRenderersThisFrame);
            }
        }

        float cameraOverlapRadius = 0.5f;
        // 1c. カメラと衝突しているオブジェクトをリストに追加 (★ここが追加機能)
        foreach (var collider in Physics.OverlapSphere(cameraPosition, cameraOverlapRadius, layerMask))
        {
            AddAllRenderersFrom(collider.gameObject, obstructedRenderersThisFrame);
        }

        // 2. 前フレームまで半透明だったが、今フレームでは対象外のオブジェクトのマテリアルを元に戻す
        var renderersToRestore = new List<Renderer>();
        foreach (var renderer in originalMaterials.Keys)
        {
            if (!obstructedRenderersThisFrame.Contains(renderer))
            {
                renderersToRestore.Add(renderer);
            }
        }
        foreach (var renderer in renderersToRestore)
        {
            ResetMaterial(renderer, originalMaterials[renderer]);
            originalMaterials.Remove(renderer);
        }

        // 3. 今フレームで新たに半透明にするオブジェクトのマテリアルを変更する
        foreach (var renderer in obstructedRenderersThisFrame)
        {
            // すでに半透明になっていなければ処理
            if (!originalMaterials.ContainsKey(renderer))
            {
                // 元のマテリアルを保存
                originalMaterials.Add(renderer, renderer.materials);
                // すべてのサブマテリアルを半透明マテリアルに置き換え
                var newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = replacementMaterial;
                }
                renderer.materials = newMaterials;
            }
        }

        // 4. `currentlyObstructedRenderers` を現在の状態で更新 (元の変数構造を維持するため)
        currentlyObstructedRenderers = obstructedRenderersThisFrame;
    }

    void ResetMaterial(Renderer renderer, Material[] originalMaterials)
    {
        if (renderer != null)
        {
            renderer.materials = originalMaterials;
        }
    }
public void TouchSystem()
{
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    float maxRayDistance = sphere.transform.lossyScale.x * 5f; // 十分な長さに設定
    RaycastHit[] allHits = Physics.RaycastAll(ray, maxRayDistance);

  Vector3 directionFromCenterToHit = new Vector3(0, 0, 0); // 初期化
            float sphereRadius = 0f;
            Vector3 spherePosition =new Vector3(0, 0, 0); // 初期化

    if (allHits.Length > 0)
    {
        allHits = allHits.OrderBy(h => h.distance).ToArray();
        RaycastHit finalHit = default(RaycastHit);
        bool foundValidTarget = false;

        foreach (RaycastHit hit in allHits)
        {
            if (hit.collider.CompareTag("Untagged")) continue;
            finalHit = hit;
            foundValidTarget = true;
            break;
        }

        if (foundValidTarget && sphereCollider != null)
        {
            // 猫のオブジェクトとの距離をチェック
            {
                //猫からこの距離以内の場所をタッチした場合は、ターゲットを動かさない
                float minDistanceToMoveTarget = catSystem.moveSpeed * 6f; // ここで距離の閾値を設定
                float distanceToCat = Vector3.Distance(finalHit.point, catSystem.transform.position);
                if (distanceToCat < minDistanceToMoveTarget)
                {
                    Debug.Log("猫に近すぎるためターゲットを移動しません。距離: " + distanceToCat);
                    return; // ここで処理を中断
                }
            }

            // --- ターゲットの移動処理 ---
            directionFromCenterToHit = (finalHit.point - sphere.transform.position).normalized;
            sphereRadius = sphereCollider.radius * GetMaxAbsScale(sphere.transform.lossyScale);
            spherePosition = sphere.transform.position + (directionFromCenterToHit * sphereRadius);

        }

    }
     //タッチをした箇所が　画面の上部20%以内の場合は、ターゲットを球体の反対側に移動させる 反対とは、Z軸のみ「-」にする
    else if (Input.mousePosition.y > Screen.height * 0.7f)
    {
        Debug.Log("画面上部20%以内をタッチしたため、ターゲットを球体の反対側に移動させます。");
        directionFromCenterToHit = (mainCamera.transform.position - sphere.transform.position).normalized;
        directionFromCenterToHit.z *= -1; // Z軸を反転
        sphereRadius = sphereCollider.radius * GetMaxAbsScale(sphere.transform.lossyScale);
        spherePosition = sphere.transform.position + (directionFromCenterToHit * sphereRadius);
    }

    targetObject.transform.position = spherePosition;
            targetObject.transform.SetParent(sphere.transform);

            // 向きの調整
            targetObject.transform.up = directionFromCenterToHit; // 球体中心から外側へのベクトル

            // 球体を回転させる
            RotateSphereToFaceTarget();
}

    // --- RotateSphereToFaceTarget メソッドの修正・追加 ---
/// <summary>
/// targetObjectがカメラの正面に来るように球体を滑らかに回転させる
/// </summary>
void RotateSphereToFaceTarget()
{
    if (sphere == null || targetObject == null || mainCamera == null)
        return;

    Vector3 sphereCenter = sphere.transform.position;

    Vector3 currentTargetDir =
        (targetObject.transform.position - sphereCenter).normalized;

    Vector3 goalDir = (transform.up * sphereCollider.radius - sphereCenter).normalized;

    Quaternion rotationToAdd =
        Quaternion.FromToRotation(currentTargetDir, goalDir);

    Quaternion targetRotation =
        rotationToAdd * sphere.transform.rotation;

    sphere.transform.DOKill();

    float sphereRotateDuration =
        Mathf.Clamp(
            Quaternion.Angle(sphere.transform.rotation, targetRotation) / 180f,
            2f, 5f);

    sphere.transform
        .DORotateQuaternion(targetRotation, sphereRotateDuration)
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            // Tween終了時に最終角度を強制的に合わせる
            sphere.transform.rotation = targetRotation;
        });
}

    // ... (GetMaxAbsScale, GameEnd, GameEndCoroutine, SphereSet, AddAllRenderersFrom メソッドは変更なし) ...
    /// <summary>
    /// ロススケールの各軸の絶対値の最大値を取得する
    /// </summary>
    /// <param name="lossyScale"></param>
    /// <returns></returns>
    float GetMaxAbsScale(Vector3 lossyScale)
    {
        return Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
    }
    void GameEnd()
    {
        if (!catSystem.FindforFriends()) return;
        if (!playerData.catFound.Contains(findCatData))
        {
            playerData.catFound.Add(findCatData);
        }
        playerData.Save();
        isPlaying = false;
        MissionSystem.missionSystem.missionUISystem.ShowCatInfoUI(catSystem.touchCatData); // ミッション終了の処理を呼び出す
    }

/// <summary>
/// 球体と猫のオブジェクトをセットする
/// </summary>
/// <param name="catPrefab"></param>
/// <param name="sphereSpec"></param>
    public void SphereSet(SphereSpec sphereSpec)
    {
        //時間関係
        Debug.Log("change Sphere to " + sphereSpec.name);
        // スフィアを変更する
        GameObject newSphere = Instantiate(sphereSpec.Sphere);
        Debug.Log("instantiate  complete");
        if (sphere != null)
        {
            DestroyImmediate(sphere);
        }
        sphere = newSphere;
        sphereCollider = sphere.GetComponent<SphereCollider>();
        playerData.currentSphereSpec = sphereSpec;
        int timerandom = Random.Range(0, playerData.currentSphereSpec.dayType.Count());
        daySphereSystem.SphreSet(playerData.currentSphereSpec.dayType[timerandom]);

        weatherSystem.WeatherSet(playerData.currentSphereSpec.weatherState);
        //猫の生成位置画面真ん中　
        Vector3 insPos = sphere.transform.position + sphere.transform.up * sphereCollider.radius * sphere.transform.lossyScale.y;
        Debug.Log("insPos is " + insPos);
        GameObject cat = Instantiate(playerData.CatPrefab, insPos, targetObject.transform.rotation);
        cat.transform.SetParent(sphere.transform);
        catSystem = cat.GetComponent<CatPlayer>();

        //猫のデータを取得
        List<CatData> list = new List<CatData>();
        foreach (CatData catData in sphereSpec.findCatsDate)
        {
            if (playerData.catFound.Contains(catData)) continue;
            list.Add(catData);
        }
        if (list.Count == 0)
        {
            Debug.Log("全ての猫を見つけています");
            if (!playerData.catFound.Contains(sphereSpec.lastCatData))
            {
                Debug.Log("最後の猫を入れる");
                list.Add(sphereSpec.lastCatData);
            }
            else
            {
                Debug.Log("最後の猫を見つけています");
                list = sphereSpec.findCatsDate;
                list.Add(sphereSpec.lastCatData);
            }
        }
        int randomIndex = Random.Range(0, list.Count);
        findCatData = list[randomIndex];
        catSystem.MoveCatSet(this);
        playerData.Save();
        FindFirstObjectByType<CatUISystem>().PaperSet(sphereSpec);
        // ゲートの位置を球体の表面のランダムな位置に設定
        Vector3 randomDirection = Random.onUnitSphere; // 球体の表面上のランダムな方向
        Vector3 gatePosition = sphere.transform.position + randomDirection * sphereCollider.radius * sphere.transform.lossyScale.y; // 球体の表面上の位置
        GameObject gateObject = Instantiate(this.gateObject, gatePosition, Quaternion.identity); // ゲートオブジェクトを取得
        gateObject.transform.SetParent(sphere.transform); // ゲートを球体の子に設定
        gateObject.transform.up = randomDirection; // ゲートの上方向を球体の中心から外側に向ける
    }
private void AddAllRenderersFrom(GameObject obj, HashSet<Renderer> rendererSet)
{
    // カメラ自身やターゲット、プレイヤーキャラクターは対象外
    if (obj.transform.IsChildOf(mainCamera.transform) || obj == targetObject || (catSystem != null && obj == catSystem.gameObject))
    {
        return;
    }

    // 自分自身とすべての子からRendererを取得して追加
    var renderers = obj.GetComponentsInChildren<Renderer>();
    foreach (var r in renderers)
    {
        rendererSet.Add(r);
    }
}

}