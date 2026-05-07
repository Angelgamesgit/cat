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
    public bool GamePlaying;
    public PlayerData playerData;
    
    public FoodDeliverySystem foodDeliverySystem;
    public FoodDeliveryUISystem foodDeliveryUISystem;

    [SerializeField]
    GameObject catTargetObject;

    [SerializeField]
    public GameObject sphere;

    [SerializeField]
    Transform skyDome;

    [SerializeField]
    GameObject weatherpoint;

    public Camera mainCamera;
    // --- 変数宣言部に追加 ---
[Header("Flick Settings")]
private int touchFrameCount;
private int flickFrameThreshold; // フリックと見渡しの判定フレーム数
private Vector3 lastMousePosition;
[Header("Camera Look Settings")]
[SerializeField] private float lookSensitivity = 0.2f; // 見渡しの感度
private float cameraYaw;   // 左右の回転角
private float cameraPitch; // 上下の回転角

[SerializeField] private float minPitch = -30f;      // 下方向の制限
[SerializeField] private float maxPitch = 60f;       // 上方向の制限


[Tooltip("フリックによる回転の感度")]
[SerializeField] private float flickSensitivity;

private Vector3 touchStartPos;
private bool isFlicking;
    // 初期カメラ位置と回転を保存する変数
    Vector3 cameraPos;
    Quaternion cameraRotation;

    [Header("Camera Settings")]
    [Tooltip("猫の真上にカメラを配置する際の距離")]
    [SerializeField] private float cameraInitialUpDistance;
    [Tooltip("猫の正面にカメラを配置する際の距離")]
    [SerializeField] private float cameraFrontDistance;
    [Tooltip("猫の正面にカメラを配置する際の高さ")]
    [SerializeField] private float cameraFrontHeight;

    [Tooltip("回転の速さを調整する係数")]
    float rotationSpeedMultiplier;

    //見つける猫のオブジェクト
    [SerializeField]
    GameObject findCatPrefab;
    public GameObject findCatObject;

    CatPlayer catSystem;

    [SerializeField]
    GameObject catFood;

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

    public GameObject clearPanel;

    [Tooltip("方向を示す矢印UIの RectTransform")]
    public RectTransform directionArrowUI;

    [SerializeField]
    FindCat findCat;
    public CatData findCatData;

    public enum MissionState
    {
        None,
        Play,
    }
public static MissionState missionState = MissionState.None;
    //画面上のタッチを終了し、画面の真ん中に戻すための位置情報
    Vector3 mainTargetPos;

    //猫が追いかけるターゲットのオブジェクトが指定の位置にあるかをチェック　タッチで場所を変えた場合は猫とターゲットの最低距離をなくす
    // ▼▼▼ 変更点: targetObjMoveフラグは新しいロジックでは不要となるためコメントアウト ▼▼▼
    // public bool targetObjMove;
    // ▲▲▲ 変更点終了 ▲▲▲
    [SerializeField]
    DaySphereSystem daySphereSystem;
    [SerializeField]
    WeatherSystem weatherSystem;

    public GameObject kitchenObject;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        AssignVariables();
        targetObject.SetActive(false);
        // cameraPosとcameraRotationはSphereSetで初期化される
        playerData.Load();
        SphereSet(playerData.currentSphereSpec);
        foodDeliverySystem = FindFirstObjectByType<FoodDeliverySystem>();
        Vector3 directionFromSphereCenterToObject = (targetObject.transform.position - sphere.transform.position).normalized;
        Vector3 targetPositionOnSurface = sphere.transform.position + (directionFromSphereCenterToObject * (sphereCollider.radius * sphere.transform.localScale.y));
        targetObject.transform.position = targetPositionOnSurface;
        targetObject.transform.up = (targetObject.transform.position - sphere.transform.position).normalized;
        mainTargetPos = targetObject.transform.position;
        kitchenObject = GameObject.FindGameObjectWithTag("Kitchen");

        // UIの初期化
        if (directionArrowUI == null)
        {
            Debug.LogError("Direction Arrow UIが設定されていません。");
        }
    }

    void AssignVariables()
    {
        GamePlaying = false;
        rotationSpeedMultiplier = 0.35f;
        layerMask = 1 << LayerMask.NameToLayer("Object"); ; // -1 は全てのレイヤー
        originalMaterials = new Dictionary<Renderer, Material[]>();
        currentlyObstructedRenderers = new HashSet<Renderer>();
        flickFrameThreshold = 10; // 10フレームで判定
    }

    public void GameStart()
    {
        targetObject.SetActive(true);
        GamePlaying = true;
        sphereCollider = sphere.GetComponent<SphereCollider>();
        FindCatSet();
    }
    public void FindCatSet()
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
        findCatObject.transform.up = upDirection;
        mainCamera.transform.SetParent(catSystem.transform, true);
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
        if (!GamePlaying) return;
        GameEnd();
        
        BetweenCameraAndObject();
        ShowSurfaceDirectionToTarget();
        HandleCameraLookOrTouch(); // フレーム数による判定メソッドを呼び出し
        if (Input.GetMouseButtonDown(0))
    {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 当たった相手がを持っていれば実行
            if (hit.collider.tag == "MissionBoard")
            {
                Debug.Log("ミッションボードをタッチ");
             MissionSelectSystem.missionSelectSystem.MissionSelect(); // ミッション選択システムの関数を呼び出す
            }
            if (hit.collider.tag == "Food")
            {
                Debug.Log("食べ物をタッチ");
                Food food = hit.collider.GetComponent<Food>();
                if (food != null)
                {
                    food.GetFood(this); // 食べ物のGetFoodメソッドを呼び出す
                }
            }
        }
    }
    }

void HandleCameraLookOrTouch()
{
    // 指が触れた瞬間
    if (Input.GetMouseButtonDown(0))
    {
        touchFrameCount = 0;
        lastMousePosition = Input.mousePosition;
        
        // 現在のカメラの角度を初期値として取得
        Vector3 currentRotation = mainCamera.transform.localEulerAngles;
        cameraYaw = currentRotation.y;
        cameraPitch = (currentRotation.x > 180) ? currentRotation.x - 360 : currentRotation.x;
    }

    // 指が触れている間
    if (Input.GetMouseButton(0))
    {
        touchFrameCount++;

        if (touchFrameCount >= flickFrameThreshold)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // マウスの移動量に応じて角度を計算
            cameraYaw += delta.x * lookSensitivity;
            cameraPitch -= delta.y * lookSensitivity; // 上下は反転させる

            // 上下の回転角度を制限（画面酔いや反転防止）
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

            // カメラのローカル回転に適用
            // 猫（親）を基準とした角度になるため、周囲を自然に見渡せます
            mainCamera.transform.localEulerAngles = new Vector3(cameraPitch, cameraYaw, 0f);
        }
        
        lastMousePosition = Input.mousePosition;
    }

    // 指を離した瞬間
    if (Input.GetMouseButtonUp(0))
    {
        
        if (touchFrameCount < flickFrameThreshold)
        {
            // 1. 目的地を移動させる既存の処理
            TouchSystem();

            // 2. カメラの回転を初期状態（cameraRotation）へ滑らかに戻す
            // DOTweenを使用して0.5秒かけて復帰
            mainCamera.transform.DOLocalRotate(cameraRotation.eulerAngles, 0.5f)
                .SetEase(Ease.OutCubic);
        }
       
        touchFrameCount = 0;
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
    /*    void CameraMove()
    {
        if (catSystem.jumpDifference == 0) return;
        Vector3 cameraJumpPos = cameraPos + (catSystem.jumpDifference * (catSystem.transform.position - sphere.transform.position).normalized);
        mainCamera.transform.DOMove(cameraJumpPos, 1f);
    }
*/
    void ShowSurfaceDirectionToTarget()
    {
        // 球体の中心座標 (ワールド座標)
        Vector3 sphereCenter = sphere.transform.position + sphereCollider.center;

        // 球体の上にいるオブジェクトと目標オブジェクトの位置
        Vector3 onSpherePos = targetObject.transform.position;
        Vector3 targetPos = findCatObject.transform.position;

        // 球体の中心から見た、それぞれのオブジェクトへのベクトル
        Vector3 fromCenterToOnSphere = onSpherePos - sphereCenter;
        Vector3 fromCenterToTarget = targetPos - sphereCenter;

        // 法線ベクトル（球体の中心からOnSphereObjectへ）
        Vector3 normal = fromCenterToOnSphere.normalized;

        // 目標への方向ベクトル（球体の中心から）
        Vector3 targetDirection = fromCenterToTarget.normalized;

        // 法線ベクトルと目標方向ベクトルの外積で、回転軸を求める
        Vector3 rotationAxis = Vector3.Cross(normal, targetDirection).normalized;

        // 回転軸と法線ベクトルの外積で、球面上での移動方向（接線ベクトル）を求める
        Vector3 surfaceDirection = Vector3.Cross(rotationAxis, normal).normalized;

        // 矢印UIをスクリーン座標で配置する場合
        Vector3 onSphereScreenPos = mainCamera.WorldToScreenPoint(onSpherePos + normal * arrowOffset);


        // 球面上での移動方向を示すワールド座標上の点
        Vector3 surfaceDirectionWorldPos = onSpherePos + surfaceDirection * arrowOffset;
        Vector3 surfaceDirectionScreenPos = mainCamera.WorldToScreenPoint(surfaceDirectionWorldPos);

        // 方向ベクトルを計算 (スクリーン座標系)
        Vector2 directionVectorScreen = surfaceDirectionScreenPos - onSphereScreenPos;

        // 角度を計算
        float angle = Mathf.Atan2(directionVectorScreen.y, directionVectorScreen.x) * Mathf.Rad2Deg;

        // 矢印UIの回転を設定
        directionArrowUI.eulerAngles = new Vector3(0, 0, angle);
    }


public void TouchSystem()
{
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    float maxRayDistance = sphere.transform.lossyScale.x * 5f; // 十分な長さに設定
    RaycastHit[] allHits = Physics.RaycastAll(ray, maxRayDistance);

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
            if (catSystem != null)
                {
                //猫からこの距離以内の場所をタッチした場合は、ターゲットを動かさない
                float minDistanceToMoveTarget = 1.5f;
                float distanceToCat = Vector3.Distance(finalHit.point, catSystem.transform.position);
                if (distanceToCat < minDistanceToMoveTarget)
                {
                    Debug.Log("猫に近すぎるためターゲットを移動しません。距離: " + distanceToCat);
                    return; // ここで処理を中断
                }
            }

            // --- ターゲットの移動処理 ---
            Vector3 directionFromCenterToHit = (finalHit.point - sphere.transform.position).normalized;
            float sphereRadius = sphereCollider.radius * GetMaxAbsScale(sphere.transform.lossyScale);
            Vector3 spherePosition = sphere.transform.position + (directionFromCenterToHit * sphereRadius);

            // 位置と親子関係の設定
            targetObject.transform.position = spherePosition;
            targetObject.transform.SetParent(sphere.transform);

            // 向きの調整
            targetObject.transform.up = directionFromCenterToHit; // 球体中心から外側へのベクトル

            // 球体を回転させる
            RotateSphereToFaceTarget();
        }
    }
// 画面をタッチした瞬間に、タッチ位置からカメラに向かってレイを飛ばし、最初に当たったオブジェクトを処理する

   
}
    // --- RotateSphereToFaceTarget メソッドの修正・追加 ---
/// <summary>
/// targetObjectがカメラの正面に来るように球体を滑らかに回転させる
/// </summary>
void RotateSphereToFaceTarget()
{
    if (sphere == null || targetObject == null || mainCamera == null) return;

    Vector3 sphereCenter = sphere.transform.position;

    // 現在のターゲット方向（球体中心から見たターゲットの位置）
    Vector3 currentTargetDir = (targetObject.transform.position - sphereCenter).normalized;
    // 目標の方向（球体中心から見たカメラの位置）
    Vector3 goalDir = (mainCamera.transform.position - sphereCenter).normalized;

    // 現在の方向から目標方向への差分回転を計算
    Quaternion rotationToAdd = Quaternion.FromToRotation(currentTargetDir, goalDir);
    // 球体の最終的な目標回転値
    Quaternion targetRotation = rotationToAdd * sphere.transform.rotation;

        // ★DOTweenを使用して滑らかに回転
        // 進行中の回転があれば停止させる（Kill）
        sphere.transform.DOKill();
    
    // Ease.OutCubic を使うことで、目標（正面）に近づくほどゆっくり減速します
    float sphereRotateDuration = 2.5f;
    sphere.transform.DORotateQuaternion(targetRotation, sphereRotateDuration)
        .SetEase(Ease.OutCubic);
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
        GamePlaying = false;
        //演出へ
        StartCoroutine(GameEndCoroutine());
    }

    IEnumerator GameEndCoroutine()
    {
        catSystem.transform.LookAt(catTargetObject.transform.position);
        catTargetObject.SetActive(false);
        Vector3 targetPos = catSystem.transform.position + (catSystem.transform.up * 2f) - (catSystem.transform.forward * 3f);
        Vector3 targetRot = catTargetObject.transform.position - targetPos;
        Vector3 selfToCenter = targetPos - sphere.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetRot, selfToCenter);
        Quaternion catTargetRotation = Quaternion.LookRotation(catTargetObject.transform.position - catSystem.transform.position, selfToCenter);
        mainCamera.transform.DOMove(targetPos, 2f);
        mainCamera.transform.DORotate(targetRotation.eulerAngles, 2f);
        catTargetObject.transform.DORotate(catTargetRotation.eulerAngles, 2f);
        catSystem.CatAnimationMove(CatSystem.AnimState.idle);
        yield return new WaitForSeconds(2.25f);
        clearPanel.SetActive(true);
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
        Vector3 insPos = Vector3.Lerp(sphere.transform.position, mainCamera.transform.position, (float)sphereCollider.radius * sphere.transform.lossyScale.x / Vector3.Distance(sphere.transform.position, mainCamera.transform.position));
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