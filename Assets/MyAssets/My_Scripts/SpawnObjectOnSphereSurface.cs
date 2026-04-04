using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor; // シーンの保存とアセットデータベースへのアクセスに必要な名前空間
#endif

public class SpawnObjectOnSphereSurface : MonoBehaviour
{
#if UNITY_EDITOR
    public enum createObjectTags
    {
        forest,
        FantasyLand,
        night,
        test,
        rock,
        grass,
        woodwall,
        flower,
        plant,
        blackrock,
        mountain,
        redrock,
        survival,
        mashroom,
        farming,
        loghouse,
        sea,
        sand,
        magicitem,
        fense,
        tree,
        snow,
        snowmat,
        snowmountain,
        tree_autumn,
        road_rock,
        rain,
        lamp,
        stoneroad,
        rainflower,
        desert,
        desertprops,
        sandrock,
        christmas,
        shrine
    }
   
   public createObjectTags createObjectTag;
   [Header("サイズの倍数 最小値")]
   [SerializeField]
   float minmultiply;
   [Header("サイズの倍数 最大値")]
   [SerializeField]
   float maxmultiply;
   [Header("オブジェクト間の間隔")]
   [SerializeField]
   float objectSpacing;   // オブジェクト間の間隔
   [Header("ランダムの際オブジェクトの生成個数")]
   [SerializeField]
   int randomspawnCount;
[Range(0,1)]
public float areaPercentage;// 使用する球面積の割合（例：10%なら10）

[Tooltip("円周上に生成するオブジェクトの数。")]
    public int numberOfObjects = 12;

    [Tooltip("円の位置を決定する割合。0で球体の頂点（ローカルY+極）、1で赤道。")]
    [Range(0f, 2f)]
    public float circleLatitudeRatio = 0.5f; // 0.5 で緯度45度付近の円

    public bool turn_Left, turn_Right, turn_Back;

    [Tooltip("生成するオブジェクトの基点を球体表面からどれだけ浮かせるかのワールド単位オフセット。")]
    public float heightOffsetFromSurface = 0f;

    [SerializeField]
    bool ringsamething;
    [SerializeField]
    int ringsamethingnum;



    void SaveScene()
    {
        // シーンを保存
        EditorUtility.SetDirty(gameObject);
        AssetDatabase.SaveAssets();
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("シーンを保存しました。");
    }
    
    // Inspectorのコンテキストメニューから実行できる関数
    [ContextMenu("Spawn Objects and Save Scene")]
    public  void SpawnObjectsAndSave()
    {
        string spawnObjectsFolderName;
        // プレハブを格納するリスト
        List<GameObject> spawnableObjects = new List<GameObject>();
        spawnObjectsFolderName = "StageSpawnObjects";
        // TagでオブジェクトAを検索
        GameObject sphere = gameObject;
        GameObject parent = Instantiate(new GameObject(createObjectTag.ToString()));
        
        parent.transform.SetParent(transform);
        parent.transform.localScale = new Vector3(1,1,1);
        parent.name = createObjectTag.ToString();
            // Sphere Colliderを持つオブジェクトのみを対象
        if (sphere == null)
        {
            return;
        }

        // オブジェクトAからSphere Colliderを取得
        SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            Debug.LogError($"オブジェクト '{sphere.name}' に Sphere Collider がアタッチされていません。");
            return;
        }
        float sphereRadius = sphereCollider.radius * Mathf.Max(sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z); // ローカルスケールを考慮

        // 指定されたフォルダーからGameObjectアセットを検索
        string folderPath = Path.Combine("Assets", spawnObjectsFolderName + "/" + createObjectTag.ToString());
       
        string[] guids = AssetDatabase.FindAssets($"t:GameObject", new string[] { folderPath });
       
        if (guids.Length == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' にGameObjectアセットが見つかりません。");
            return;
        }

        // 検索されたアセットをロードしてリストに格納
        spawnableObjects.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
            spawnableObjects.Add(prefab);
            }
        }
        if (spawnableObjects.Count == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' からロードできるGameObjectアセットがありません。");
            return;
        }

       Vector3 areaCenterDirection;
       float maxAngleFromCenter; // エリアの広がり角度（ラジアン）


        // ランダムにエリア中心の方向を決める
        areaCenterDirection = Random.onUnitSphere;

        // 球の表面積の10%に相当する開き角を計算
        maxAngleFromCenter = Mathf.Acos(1 - (2 * areaPercentage));
        Debug.Log("maxAngleFromCenter  " + maxAngleFromCenter);
      
        float objectDiameter = objectSpacing;
        float  latitudeStep = Mathf.Asin(objectDiameter / (2f * sphereRadius)) * 2f;
         
     
        for (float latitude = 0; latitude <= Mathf.PI; latitude += latitudeStep)
        {
            float latitudeRadius = Mathf.Sin(latitude) * sphereRadius;
            int numLongitudes = Mathf.Max(1, Mathf.RoundToInt((2f * Mathf.PI * latitudeRadius) / objectSpacing));

            for (int i = 0; i < numLongitudes; i++)
            {
                float longitude = (2f * Mathf.PI / numLongitudes) * i;
                Vector3 position = new Vector3(
                    sphereRadius * Mathf.Sin(latitude) * Mathf.Cos(longitude),
                    sphereRadius * Mathf.Cos(latitude),
                    sphereRadius * Mathf.Sin(latitude) * Mathf.Sin(longitude)
                );

                Vector3 normalizedDirection = position.normalized;
                float angleFromCenter = Mathf.Acos(Vector3.Dot(normalizedDirection, areaCenterDirection));

        if (angleFromCenter <= maxAngleFromCenter)
            {
            int randomIndex = Random.Range(0, spawnableObjects.Count);
            Vector3 upDirection = (position - transform.position).normalized;
            GameObject objectToSpawn = spawnableObjects[randomIndex];
            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(objectToSpawn) as GameObject; 
            spawnedObject.transform.position = position;
            spawnedObject.transform.up = upDirection; 
            float scaleMultiply = Random.Range(minmultiply,maxmultiply);
            spawnedObject.transform.localScale *= scaleMultiply;
            spawnedObject.transform.SetParent(parent.transform);
            }
        }
        }
        // シーンを保存 
        SaveScene();
        Debug.Log("オブジェクトを生成し、シーンを保存しました。");
    }

    [ContextMenu("Random Spawn  and Save Scene")]
    public void RandomSpawnObjectsAndSave()
    {
        string spawnObjectsFolderName;
        // プレハブを格納するリスト
        List<GameObject> spawnableObjects = new List<GameObject>();
        spawnObjectsFolderName = "StageSpawnObjects";
        // TagでオブジェクトAを検索
        GameObject sphere = gameObject;
        GameObject parent = Instantiate(new GameObject(createObjectTag.ToString()));
        parent.transform.SetParent(transform);
        parent.name = createObjectTag.ToString();
        // Sphere Colliderを持つオブジェクトのみを対象

        // オブジェクトAからSphere Colliderを取得
        SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();

        float sphereRadius = sphereCollider.radius * Mathf.Max(sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z); // ローカルスケールを考慮

        // 指定されたフォルダーからGameObjectアセットを検索
        string folderPath = Path.Combine("Assets", spawnObjectsFolderName + "/" + createObjectTag.ToString());

        string[] guids = AssetDatabase.FindAssets($"t:GameObject", new string[] { folderPath });


        // 検索されたアセットをロードしてリストに格納
        spawnableObjects.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                spawnableObjects.Add(prefab);
            }
        }

        if (spawnableObjects.Count == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' からロードできるGameObjectアセットがありません。");
            return;
        }

        Vector3 areaCenter = Random.onUnitSphere; // 中心方向をランダムに決定

        float maxAngle = Mathf.Acos(1f - 2f * areaPercentage) * Mathf.Rad2Deg; // 10%のエリアに相当する最大角度

        int spawned = 0;
        int tries = 0;
        while (spawned < randomspawnCount && tries < 100000)
        {
            tries++;
            Vector3 direction = Random.onUnitSphere;
            float angle = Vector3.Angle(areaCenter, direction);
            if (angle <= maxAngle)
            {
                spawned++;
                Vector3 position = transform.position + (direction * (sphereRadius + heightOffsetFromSurface));
                int randomIndex = Random.Range(0, spawnableObjects.Count);
                Vector3 upDirection = (position - transform.position).normalized;
                GameObject objectToSpawn = spawnableObjects[randomIndex];
                GameObject spawnedObject = PrefabUtility.InstantiatePrefab(objectToSpawn) as GameObject;
                spawnedObject.transform.position = position;
                spawnedObject.transform.up = upDirection;
                float scaleMultiply = Random.Range(minmultiply, maxmultiply);
                spawnedObject.transform.localScale *= scaleMultiply;
                spawnedObject.transform.SetParent(parent.transform);
            }
        }
        SaveScene();
        Debug.Log("オブジェクトを生成し、シーンを保存しました。");   
}

[ContextMenu("Spawn Grass and Save Scene")]
 public  void SpawnGrassAndSave()
{
     
    //[Tooltip("生成するオブジェクトBが格納されているAssetフォルダー名")]
     string spawnObjectsFolderName;
 
        // プレハブを格納するリスト
    List<GameObject> spawnableObjects = new List<GameObject>();
        
        spawnObjectsFolderName = "StageSpawnObjects";

        // TagでオブジェクトAを検索
        GameObject sphere = gameObject;
        GameObject parent = Instantiate(new GameObject("grass"));
        parent.transform.SetParent(transform);
        parent.name = "grass";
        // Sphere Colliderを持つオブジェクトのみを対象
       
        if (sphere == null)
        {
            return;
        }

        // オブジェクトAからSphere Colliderを取得
        SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            Debug.LogError($"オブジェクト '{sphere.name}' に Sphere Collider がアタッチされていません。");
            return;
        }
        float sphereRadius = sphereCollider.radius * Mathf.Max(sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z); // ローカルスケールを考慮

        // 指定されたフォルダーからGameObjectアセットを検索
        string folderPath = Path.Combine("Assets", spawnObjectsFolderName + "/grass");
        string[] guids = AssetDatabase.FindAssets($"t:GameObject", new string[] { folderPath });
       
        if (guids.Length == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' にGameObjectアセットが見つかりません。");
            return;
        }

        // 検索されたアセットをロードしてリストに格納
        spawnableObjects.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
           {
                spawnableObjects.Add(prefab);
           }
        }
         
        if (spawnableObjects.Count == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' からロードできるGameObjectアセットがありません。");
            return;
        }
        
        float objectDiameter = spawnableObjects[0].transform.localScale.x * 0.45f;
        float angleStep = Mathf.Asin(objectDiameter / (2f * sphereRadius)) * 2f; // 弧度法
       
         // 球面上にオブジェクトを均等に配置する (フィボナッチグリッドのようなアプローチ)
        for (float latitude = 0; latitude <= Mathf.PI; latitude += angleStep)
        {
            float latitudeRadius = Mathf.Sin(latitude) * sphereRadius;
            int numLongitudes = Mathf.Max(1, Mathf.RoundToInt((2f * Mathf.PI * latitudeRadius) / objectDiameter));

            for (int i = 0; i < numLongitudes; i++)
            {
                float longitude = (2f * Mathf.PI / numLongitudes) * i;

                Vector3 position = new Vector3(
                    sphereRadius * Mathf.Sin(latitude) * Mathf.Cos(longitude),
                    sphereRadius * Mathf.Cos(latitude),
                    sphereRadius * Mathf.Sin(latitude) * Mathf.Sin(longitude)
                );
                          
            Vector3 upDirection = (position - transform.position).normalized;
            GameObject objectToSpawn = spawnableObjects[0];
            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(objectToSpawn) as GameObject; 
            spawnedObject.transform.position = position;
            spawnedObject.transform.up = upDirection;           
            spawnedObject.transform.SetParent(parent.transform);
            }
        }
        // シーンを保存 
        SaveScene();
        Debug.Log("オブジェクトを生成し、シーンを保存しました。");
}
    [ContextMenu("Ring Spawn and Save Scene")]
    public void SpawnRingAndSave()
    {
        // プレハブからサイズを取得
        string spawnObjectsFolderName;
        // プレハブを格納するリスト
        List<GameObject> spawnableObjects = new List<GameObject>();
        spawnObjectsFolderName = "StageSpawnObjects";
        // TagでオブジェクトAを検索

     
        // Sphere Colliderを持つオブジェクトのみを対象

        // オブジェクトAからSphere Colliderを取得
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        // 指定されたフォルダーからGameObjectアセットを検索
        string folderPath = Path.Combine("Assets", spawnObjectsFolderName + "/" + createObjectTag.ToString());

        string[] guids = AssetDatabase.FindAssets($"t:GameObject", new string[] { folderPath });

        if (guids.Length == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' にGameObjectアセットが見つかりません。");
            return;
        }

        // 検索されたアセットをロードしてリストに格納
        spawnableObjects.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                spawnableObjects.Add(prefab);
            }
        }
        if (spawnableObjects.Count == 0)
        {
            Debug.LogError($"フォルダー '{folderPath}' からロードできるGameObjectアセットがありません。");
            return;
        }


    // --- 2. 新しい親オブジェクトを生成 ---
        GameObject parentObject = new GameObject(createObjectTag.ToString() + "Ring");
        parentObject.transform.SetParent(this.transform, false);
        parentObject.transform.localPosition = Vector3.zero;
        parentObject.transform.localRotation = Quaternion.identity;
        parentObject.transform.localScale = Vector3.one;

        //--- 3. ローカル球体パラメータの取得 ---
        Vector3 localSphereCenter;
        float localSphereRadius;
        if (sphereCollider != null)
        {
            localSphereCenter = sphereCollider.center;
            localSphereRadius = sphereCollider.radius;
        }
        else
        {
            Debug.LogWarning("SphereCollider not found on this GameObject. Using approximate radius from local scale and (0,0,0) as local center.", this);
            localSphereCenter = Vector3.zero;
            localSphereRadius = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 0.5f;
        }

        // --- 4. 円のローカルパラメータ計算 (軸はローカルY軸固定) ---
        float phi_rad = circleLatitudeRatio * Mathf.PI * 0.5f;
        float yOffsetFromLocalSphereCenter = localSphereRadius * Mathf.Cos(phi_rad);
        float actualCircleLocalRadius = localSphereRadius * Mathf.Sin(phi_rad);
        Vector3 circlePlaneCenterLocal = localSphereCenter + (Vector3.up * yOffsetFromLocalSphereCenter);

        // --- 5. 円平面のローカル基底ベクトル ---
        // 円の法線はローカルY軸 (Vector3.up) と定義しているため、
        // 円平面のタンジェントベクトルはローカルX軸 (Vector3.right) とローカルZ軸 (Vector3.forward) で構成できる。
        Vector3 localTangentU = Vector3.right;   // (1, 0, 0)
        Vector3 localTangentV = Vector3.forward; // (0, 0, 1)
        // これらは正規化されており、互いに直交し、Vector3.up とも直交する。

        // --- 6. オブジェクトを円周上に生成・配置・回転 ---
        for (int i = 0; i < numberOfObjects; i++)
        {
            float angleRad = (2f * Mathf.PI / numberOfObjects) * i;

            // 6a. 円周上の点のローカル座標を計算 (球のローカル中心基準ではない、円の中心基準の平面上)
            Vector3 pointOnCirclePlaneLocalOffset = localTangentU * actualCircleLocalRadius * Mathf.Cos(angleRad) +
                                                    localTangentV * actualCircleLocalRadius * Mathf.Sin(angleRad);

            Vector3 pointOnCirclePlaneLocal = circlePlaneCenterLocal + pointOnCirclePlaneLocalOffset;


            Vector3 surfaceNormalLocal = (pointOnCirclePlaneLocal - localSphereCenter).normalized;


            // 6c. 球体表面上の点のローカル座標
            Vector3 pointOnSphereSurfaceLocal = localSphereCenter + (surfaceNormalLocal * localSphereRadius);

            // 6d. 球体表面上の点のワールド座標
            Vector3 pointOnSphereSurfaceWorld = transform.TransformPoint(pointOnSphereSurfaceLocal);

            // 6e. 球体表面の法線ベクトル（ワールド座標系）
            // これがオブジェクトの「上方向」になる。
            Vector3 upDirectionPoint = localSphereCenter - pointOnSphereSurfaceWorld; // ローカルY軸
            Vector3 objectUpDirectionWorld = transform.TransformDirection(surfaceNormalLocal).normalized;
            if (objectUpDirectionWorld == Vector3.zero) objectUpDirectionWorld = transform.up; // 安全策

            // 6f. 最終的なスポーン位置 (ワールド座標)
            Vector3 spawnPosition = pointOnSphereSurfaceWorld + (objectUpDirectionWorld * heightOffsetFromSurface);
            int ran = Random.Range(0, spawnableObjects.Count);

            if (ringsamething)
            {
                if (ringsamethingnum < spawnableObjects.Count)
                {
                    ran = ringsamethingnum;
                }

            }
            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(spawnableObjects[ran]) as GameObject; ;
            spawnedObject.transform.position = spawnPosition;
            spawnedObject.transform.SetParent(parentObject.transform);
            spawnedObject.transform.rotation = Quaternion.identity;
            spawnedObject.name = $"{spawnedObject.name}_circle_{i}";
            float s = Random.Range(minmultiply, maxmultiply);
            spawnedObject.transform.localScale *= s;
            // 6h. オブジェクトの向きを設定
            // Y方向 (Up): objectUpDirectionWorld (球体の中心方向の逆)
            // Z方向 (Forward): 円の中心方向の逆 (円の中心からオブジェクトへ向かう方向)

            Vector3 objectForwardDirectionWorld;
            if (actualCircleLocalRadius > Mathf.Epsilon) // 円の半径が0より大きい場合
            {
                // 円の中心のワールド座標
                Vector3 circlePlaneCenterWorld = transform.TransformPoint(circlePlaneCenterLocal);
                // 円の中心からオブジェクトの配置位置(表面)へ向かうベクトル
                objectForwardDirectionWorld = (pointOnSphereSurfaceWorld - circlePlaneCenterWorld).normalized;
            }
            else // 円の半径が0 (極に配置) の場合
            {
                // 極では明確な「円の中心から外向き」がないため、代替の前方方向を定義
                // 例えば、球体のローカルX軸をワールドに変換した方向など
                objectForwardDirectionWorld = transform.TransformDirection(localTangentU).normalized;
            }

            if (objectForwardDirectionWorld == Vector3.zero) // 安全策
            {
                objectForwardDirectionWorld = transform.forward;
            }

            // LookRotationの堅牢性を高める
            // もしobjectForwardDirectionWorldがobjectUpDirectionWorldとほぼ平行なら、前方ベクトルを調整
            if (Mathf.Abs(Vector3.Dot(objectForwardDirectionWorld, objectUpDirectionWorld)) > 0.999f)
            {
                // objectUpDirectionWorld (法線) と直交するベクトルを前方とする
                // 例えば、ワールドX軸を法線に射影して除去したベクトル、またはtransform.rightなど
                Vector3 alternativeForward = Vector3.Cross(objectUpDirectionWorld, transform.right); // 法線とワールド右軸の外積
                if (alternativeForward.sqrMagnitude < 0.001f) // もしワールド右軸も法線と平行なら
                {
                    alternativeForward = Vector3.Cross(objectUpDirectionWorld, transform.forward); // ワールド前軸と法線の外積
                }
                objectForwardDirectionWorld = alternativeForward.normalized;
                if (objectForwardDirectionWorld == Vector3.zero) objectForwardDirectionWorld = transform.forward; //最終手段
            }
            Quaternion baseRotation = new Quaternion();
            if (circleLatitudeRatio <= 1)
            {
                 baseRotation = Quaternion.LookRotation(objectForwardDirectionWorld, objectUpDirectionWorld) * Quaternion.Euler(90 * circleLatitudeRatio, 0, 0);
            }
            else
            {
                baseRotation = Quaternion.LookRotation(objectForwardDirectionWorld, objectUpDirectionWorld) * Quaternion.Euler( 180 -( 90 * circleLatitudeRatio), 0, 0) ;
            }
            if (circleLatitudeRatio == 1)
            {
                baseRotation = Quaternion.LookRotation(objectForwardDirectionWorld, objectUpDirectionWorld);
            }
            spawnedObject.transform.rotation = baseRotation;
            if (turn_Left)
            {
                baseRotation = Quaternion.LookRotation(spawnedObject.transform.right * -1,objectUpDirectionWorld);
            }
            else if (turn_Right)
            {
                baseRotation = Quaternion.LookRotation(spawnedObject.transform.right,objectUpDirectionWorld);
            }
            else if (turn_Back)
            {
                baseRotation = Quaternion.LookRotation(spawnedObject.transform.forward * -1,-objectUpDirectionWorld);
            }
            spawnedObject.transform.rotation = baseRotation;
        }

        Debug.Log($"{numberOfObjects} objects generated on sphere circle under '{parentObject.name}'.");

        SaveScene();
  }

  
    // インスペクターからボタンで実行できるようにするためのヘルパー
    // (この機能は必須ではないが、利便性のために残すことも可能)
    [Header("Ring Create Action")]
    [SerializeField] bool _generateAction = false; // ボタンのように使うためのbool

    void OnValidate()
    {
        if (_generateAction)
        {
            // OnValidateでの即時実行はUndoやシーンのダーティフラグ管理で問題を起こすことがあるため
            // EditorApplication.delayCall を使うか、ContextMenu を推奨
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && Application.isEditor && !Application.isPlaying) // エディタ再生中でないことを確認
                {
                    Debug.Log("GenerateObjects called via OnValidate (delayed).");
                    SpawnRingAndSave();
                }
            };
            _generateAction = false; // 実行後はフラグを戻す
        }
    }


    // Sceneビューで球体の半径を視覚的に確認するためのGizmo (localScaleを考慮)
    private void OnDrawGizmosSelected()
    {
        GameObject sphere = GameObject.FindGameObjectWithTag("Sphere");
        if (sphere != null)
        {
            SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = sphere.transform.localToWorldMatrix; // ローカルスケールを考慮
                Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
            }
        }
    }
    #endif
}