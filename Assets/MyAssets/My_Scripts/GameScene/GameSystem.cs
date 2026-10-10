using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.VisualScripting;


//  UI判定に必要
public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }
    public bool isPlaying;
    [SerializeField] private CameraSystem cameraSystem;
    [SerializeField] private TouchSystem touchSystem;
    [SerializeField] private GameEventSystem gameEventSystem;

//落とし物を拾った後かどうかをチェックするフラグ　拾った後がtrue
    public bool find_LostItem;
    public PlayerData playerData;

    [SerializeField]
    public GameObject sphere;
    public GameObject lostItemPrefab;
    public Camera mainCamera;




    //見つける猫のオブジェクト
    [SerializeField]
    GameObject findCatPrefab;
[HideInInspector]
    public GameObject findCatObject;

    CatPlayer catSystem;
    [Tooltip("The sphere rotates to face this target. Objects between the camera and this target become transparent.")]
    public GameObject targetObject;

    [Tooltip("Material applied to objects between the camera and the target.")]
    public Material replacementMaterial;
SphereCollider sphereCollider;

    [SerializeField]
    FindCat findCat;
    public CatData findCatData;
public CatData[]  catData;


    //猫が追いかけるターゲットのオブジェクトが指定の位置にあるかをチェック　タッチで場所を変えた場合は猫とターゲットの最低距離をなくす
    // ▼▼▼ 変更点: targetObjMoveフラグは新しいロジックでは不要となるためコメントアウト ▼▼▼
    // public bool targetObjMove;
    // ▲▲▲ 変更点終了 ▲▲▲
    [SerializeField]
    DaySphereSystem daySphereSystem;
    [SerializeField]
    WeatherSystem weatherSystem;
    [HideInInspector]
    public LostItemData lostItemData;
    [SerializeField]
    Transform catLightTransform,targetLightTransform;
    [SerializeField]
    LightResourceSystem lightResourceSystem;
    [SerializeField]
    LightTargetSystem lightTargetSystem;
    public LightResourceSystem LightResourceSystem => lightResourceSystem;
    public LightTargetSystem LightTargetSystem => lightTargetSystem;
    public CatPlayer PlayerCat => catSystem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

        void Start()
    {
        AssignVariables();
        targetObject.SetActive(true);
        playerData.Load();
        SphereSet(playerData.currentSphereSpec);
        //星の頂点にターゲットを配置する
        Vector3 targetPositionOnSurface = sphere.transform.position + (sphere.transform.up * sphereCollider.radius * sphere.transform.localScale.y);
        targetObject.transform.position = targetPositionOnSurface;
        targetObject.transform.up = (targetObject.transform.position - sphere.transform.position).normalized;
        InitializeSystems();
        // UIの初期化
    }

        void AssignVariables()
    {
        isPlaying = false;
        if (cameraSystem == null) cameraSystem = FindFirstObjectByType<CameraSystem>();
        if (cameraSystem == null) cameraSystem = gameObject.AddComponent<CameraSystem>();
        if (mainCamera == null && cameraSystem != null) mainCamera = cameraSystem.mainCamera;
        if (touchSystem == null) touchSystem = GetComponent<TouchSystem>();
        if (touchSystem == null) touchSystem = gameObject.AddComponent<TouchSystem>();
        if (gameEventSystem == null) gameEventSystem = GetComponent<GameEventSystem>();
        if (gameEventSystem == null) gameEventSystem = gameObject.AddComponent<GameEventSystem>();
    }

    private void InitializeSystems()
    {
        cameraSystem.SetWorldReferences(mainCamera, targetObject.transform, sphere.transform, catSystem, replacementMaterial);
        touchSystem.Initialize(this, cameraSystem);
        gameEventSystem.Initialize(this, playerData, lightResourceSystem);
    }

//ボタンから呼び出される　ゲームを開始する
    public void GameStart()
    {
        targetObject.SetActive(true);
        gameEventSystem.BeginGame();
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
        findCatObject.SetActive(false);
        findCatObject.transform.up = upDirection;

        //猫のデータからみつけてくるアイテムもスフィア上に配置する
        randomDirection = Random.insideUnitSphere.normalized;
        spawnPosition = sphere.transform.position + (randomDirection * sphereRadius);
        upDirection = randomDirection.normalized;
        GameObject lostItem = Instantiate(findCat.catData.FindTreasurePrefab, spawnPosition, Quaternion.identity);
        lostItem.transform.up = upDirection;
        lostItem.transform.SetParent(sphere.transform);
    find_LostItem = false; //落とし物を拾った後のフラグを初期化
        /// <summary>
        /// 後々データから反映するもの、　スフィアのデータから猫のデータを取得して、猫の種類や見た目を変える　猫のデータから、猫の行動パターンやアニメーションを変える
        /// </summary>
        yield return null; // 猫を順番に生成するための待機時間
        }
    }

    /// <summary>
    /// カメラの位置を切り替える
    /// </summary>
    /// <param name="up">true: 猫の真上（初期位置）へ, false: 猫の正面へ</param>
    /// <param name="time">移動にかかる時間</param>
    public void cameraPosChange(bool up, float time)
    {
        if (cameraSystem != null) cameraSystem.ChangeCameraPosition(up, time, catSystem, mainCamera);
    }

    void Update()
    {
        if (!isPlaying) return;
        if (targetLightTransform != null && targetObject != null) targetLightTransform.LookAt(targetObject.transform.position);
        if (cameraSystem != null) cameraSystem.RefreshObstructions();

        if(Input.GetMouseButtonUp(0))
        {
            TouchSystem();
            if (lightResourceSystem != null) lightResourceSystem.UseLightByTouch();
        }
        
        //終了処理を確認
        GameEnd();
    }


public void TouchSystem()
    {
        if (touchSystem != null) touchSystem.ProcessTouch();
    }

public void LostItemTouched()
    {
        if (gameEventSystem != null) gameEventSystem.HandleLostItemFound();
    }
    public void GameClear()
    {
        if (gameEventSystem != null) gameEventSystem.HandleGameClear();
    }
    /// <summary>
    /// ゲーム終了処理 失敗
    /// </summary>
    public void GameEnd()
    {
        if (gameEventSystem != null) gameEventSystem.CheckGameEnd();
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
        GameObject cat = Instantiate(playerData.CatPrefab, insPos, targetObject.transform.rotation);
        cat.transform.SetParent(sphere.transform);
        catSystem = cat.GetComponent<CatPlayer>();
        if (cameraSystem != null) cameraSystem.SetWorldReferences(mainCamera, targetObject.transform, sphere.transform, catSystem, replacementMaterial);

        //猫のデータを取得
        List<CatData> list = new List<CatData>();
        foreach (CatData catData in sphereSpec.findCatsDate)
        {
            if (playerData.catFound.Contains(catData)) continue;
            list.Add(catData);
        }
        if (list.Count == 0)
        {
            Debug.Log("All cats have been found.");
            if (!playerData.catFound.Contains(sphereSpec.lastCatData))
            {
                Debug.Log("Adding the final cat.");
                list.Add(sphereSpec.lastCatData);
            }
            else
            {
                Debug.Log("The final cat has already been found.");
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
    }


}




