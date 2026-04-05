using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

[CustomEditor(typeof(ProcessMaterialData))]
public class ProcessMaterialDataEditor : Editor
{
    public enum ItemType
    {
        Garden,
        Stage
    }

    public override void OnInspectorGUI()
    {
        // デフォルトのInspector表示
        DrawDefaultInspector();

        ProcessMaterialData myScript = (ProcessMaterialData)target;

        GUILayout.Space(20);

        // Inspectorにボタンを追加
        if (GUILayout.Button("Process All Material Data Cat (Save Texture2D)"))
        {
            ProcessAllMaterialCatData(myScript);
        }
        if (GUILayout.Button("Process All Garden Item Data (Save Sprite)"))
        {
            ProcessAllGardenItemData(myScript);
        }
        if (GUILayout.Button("Process All Stage Item Data (Save Texture2D)"))
        {

        }
        if (GUILayout.Button("Process All Stage Sphere Data (Save Sprite)"))
        {
            ProcessAllSphereData(myScript);
        }
    }


    void ProcessAllMaterialCatData(ProcessMaterialData myScript)
    {
        if (myScript.targetObjectToModify_Cat == null)
        {
            Debug.LogError("Target Object To Modify が設定されていません。処理を中止します。");
            return;
        }

        if (myScript.catDataList == null || myScript.catDataList.Count == 0)
        {
            Debug.LogWarning("処理する Material Data がリストにありません。");
            return;
        }

        // テクスチャー保存フォルダが存在するか確認し、なければ作成
        string fullSaveFolderPath = Path.Combine("Assets", myScript.textureSaveFolderPath.Replace("Assets/", ""));
        if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
        {
            if (Directory.Exists(Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""))))
            {
                // Folder exists in file system but not recognized by AssetDatabase
                AssetDatabase.Refresh();
            }
            else
            {
                // Create the folder if it doesn't exist
                string[] folders = myScript.textureSaveFolderPath.Replace("Assets/", "").Split('/');
                string currentPath = "Assets";
                foreach (string folder in folders)
                {
                    string newPath = Path.Combine(currentPath, folder);
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folder);
                    }
                    currentPath = newPath;
                }
                AssetDatabase.Refresh();
            }

            if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
            {
                Debug.LogError($"テクスチャー保存フォルダー '{fullSaveFolderPath}' の作成または確認に失敗しました。処理を中止します。");
                return;
            }
        }

        Renderer targetRenderer = myScript.targetObjectToModify_Cat.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogError("Target Object To Modify に Renderer コンポーネントがありません。マテリアル変更はできません。処理を中止します。");
            return;
        }

        // 処理を開始する前に現在のマテリアルを保存しておく（必要であれば）
        // Material[] originalMaterials = targetRenderer.sharedMaterials;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("シーンに Main Camera がありません。テクスチャーの保存をスキップします。処理を中止します。");
            return;
        }

        // カメラの元のターゲットテクスチャーを保存しておく
        RenderTexture originalCameraTargetTexture = mainCamera.targetTexture;
        CameraClearFlags prevClearFlags = mainCamera.clearFlags;
        Color prevBackgroundColor = mainCamera.backgroundColor;

        Color transparentColor = Color.black; // 例として緑色を指定
        float colorTolerance = 0.2f;

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = transparentColor;

        // リストの各MaterialDataを処理
        for (int i = 0; i < myScript.catDataList.Count; i++)
        {
            CatData currentMaterialData = myScript.catDataList[i];

            if (currentMaterialData == null)
            {
                Debug.LogWarning($"リストの要素 {i} がnullです。スキップします。");
                continue;
            }

            Debug.Log($"--- MaterialData 処理中 (Texture2D 保存): {currentMaterialData.name} ({i + 1}/{myScript.catDataList.Count}) ---");

            // 1. オブジェクトのマテリアルをスクリプタブルオブジェクトから取得し変更する。
            if (currentMaterialData.targetMaterial != null)
            {
                targetRenderer.sharedMaterials = new Material[] { currentMaterialData.targetMaterial };
                Debug.Log($"'{myScript.targetObjectToModify_Cat.name}' のマテリアルを '{currentMaterialData.targetMaterial.name}' に変更しました。");
            }
            else
            {
                Debug.LogWarning($"'{currentMaterialData.name}' に Target Material が設定されていません。マテリアル変更はスキップします。");
                // マテリアルがない場合でもキャプチャを行う場合は、ここを調整
            }

            // 強制的にシーンを再描画（マテリアル変更が反映されるように）
            SceneView.RepaintAll();
            // 少し待つことで描画が完了するのを助ける場合がありますが、Editorでは保証されません。
            // System.Threading.Thread.Sleep(50);

            // 2. カメラからの景色を Texture2D にして保存し、Scriptable Objectに代入。
            //    テクスチャーの名前をスクリプタブルオブジェクトと同じ名前にする。
            //    また指定のファイルにテクスチャーを保存する。

            // キャプチャ用のレンダーテクスチャーを作成
            RenderTexture captureRT = new RenderTexture(myScript.captureResolution.x, myScript.captureResolution.y, 24, RenderTextureFormat.ARGB32);

            mainCamera.targetTexture = captureRT; // カメラの出力先をキャプチャ用レンダーテクスチャーに設定

            // カメラをレンダリング
            mainCamera.Render();
            // RenderTexture の内容を Texture2D にコピー
            RenderTexture prevActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = captureRT;
            Texture2D screenShotTexture = new Texture2D(myScript.captureResolution.x, myScript.captureResolution.y, TextureFormat.ARGB32, false);

            screenShotTexture.ReadPixels(new Rect(0, 0, myScript.captureResolution.x, myScript.captureResolution.y), 0, 0);
            screenShotTexture.Apply(); // ReadPixels後にはApplyが必要

            Color[] pixels = screenShotTexture.GetPixels(); // 全ピクセルデータを取得

            for (int s = 0; s < pixels.Length; s++)
            {
                // ピクセル色と透過色の差を計算 (各RGB要素の差の絶対値の合計)
                float colorDifference = Mathf.Abs(pixels[s].r - transparentColor.r) +
                                        Mathf.Abs(pixels[s].g - transparentColor.g) +
                                        Mathf.Abs(pixels[s].b - transparentColor.b);

                // 差が許容誤差以下なら、そのピクセルを透明にする (アルファ値を0)
                if (colorDifference <= colorTolerance)
                {
                    pixels[s].a = 0f; // アルファ値を0に
                }
            }
            screenShotTexture.SetPixels(pixels); // 変更したピクセルデータを設定
            screenShotTexture.Apply(); // 変更をテクスチャに適用

            RenderTexture.active = prevActiveRenderTexture; // アクティブなRenderTextureを元に戻す
            // カメラの出力先を元に戻す
            mainCamera.targetTexture = originalCameraTargetTexture;
            mainCamera.clearFlags = prevClearFlags;
            mainCamera.backgroundColor = prevBackgroundColor;
            // キャプチャ用のレンダーテクスチャーを破棄
            DestroyImmediate(captureRT);

            // Texture2D を PNG データにエンコード
            byte[] bytes = screenShotTexture.EncodeToPNG();

            // 保存するファイルパスを決定
            string textureFileName = currentMaterialData.name + ".png"; // Scriptable Objectと同じ名前に
            string assetPath = Path.Combine(fullSaveFolderPath, textureFileName);
            string filePath = Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""), textureFileName);

            // ファイルに書き出し
            File.WriteAllBytes(filePath, bytes);

            // Editor に新しいアセット（テクスチャー）を認識させる
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            Debug.Log($"テクスチャー '{textureFileName}' を '{assetPath}' に保存しました。");

            // 保存した Texture2D アセットをロード
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            // 3. Texture2D アセットを Scriptable Object の変数に代入し、Scriptable Object を保存。
            currentMaterialData.catIcon = CreateSpriteFromScriptableObject(savedTexture);

            // Scriptable Object の変更をマーク
            EditorUtility.SetDirty(currentMaterialData);

            // Scriptable Object の変更を保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); // Scriptable Object の変更も反映

            Debug.Log($"'{currentMaterialData.name}' Scriptable Object に Texture2D への参照を保存しました。");

            // ランタイムの Texture2D はもう不要
            DestroyImmediate(screenShotTexture);
        }

        // 処理後にシーンビューを元に戻した結果を反映
        SceneView.RepaintAll();

        Debug.Log("--- 全ての MaterialData (cat data) の処理が完了しました (Texture2D 保存) ---");
    }
    void ProcessAllGardenItemData(ProcessMaterialData myScript)
    {
        if (myScript.transformToModify_Item == null)
        {
            Debug.LogError("Target Object To Modify が設定されていません。処理を中止します。");
            return;
        }

      
        // テクスチャー保存フォルダが存在するか確認し、なければ作成
        string fullSaveFolderPath = Path.Combine("Assets", myScript.textureSaveFolderPath.Replace("Assets/", ""));
        if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
        {
            if (Directory.Exists(Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""))))
            {
                // Folder exists in file system but not recognized by AssetDatabase
                AssetDatabase.Refresh();
            }
            else
            {
                // Create the folder if it doesn't exist
                string[] folders = myScript.textureSaveFolderPath.Replace("Assets/", "").Split('/');
                string currentPath = "Assets";
                foreach (string folder in folders)
                {
                    string newPath = Path.Combine(currentPath, folder);
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folder);
                    }
                    currentPath = newPath;
                }
                AssetDatabase.Refresh();
            }

            if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
            {
                Debug.LogError($"テクスチャー保存フォルダー '{fullSaveFolderPath}' の作成または確認に失敗しました。処理を中止します。");
                return;
            }
        }


        // 処理を開始する前に現在のマテリアルを保存しておく（必要であれば）
        // Material[] originalMaterials = targetRenderer.sharedMaterials;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("シーンに Main Camera がありません。テクスチャーの保存をスキップします。処理を中止します。");
            return;
        }

        // カメラの元のターゲットテクスチャーを保存しておく
        RenderTexture originalCameraTargetTexture = mainCamera.targetTexture;
        CameraClearFlags prevClearFlags = mainCamera.clearFlags;
        Color prevBackgroundColor = mainCamera.backgroundColor;

        Color transparentColor = Color.black; // 例として緑色を指定
        float colorTolerance = 0.01f;

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = transparentColor;

        /* リストの各MaterialDataを処理
        for (int i = 0; i < myScript.gardenItemList.Count; i++)
        {

            Garden_ItemSO currentObjectData = myScript.gardenItemList[i];
            if (currentObjectData == null)
            {
                Debug.LogWarning($"リストの要素 {i} がnullです。スキップします。");
                continue;
            }



            Debug.Log($"--- MaterialData 処理中 (Texture2D 保存): {currentObjectData.name} ({i + 1}/{myScript.catDataList.Count}) ---");

            // 1. オブジェクトをスクリプタブルオブジェクトから取得し変更する。
            GameObject itemObject = myScript.transformToModify_Item.gameObject;
            if (currentObjectData.prefab != null)
            {
                itemObject = Instantiate(currentObjectData.prefab, myScript.transformToModify_Item.position, myScript.transformToModify_Item.rotation);
            }
            else
            {
                Debug.LogWarning($"'{currentObjectData.name}' に Target prefab が設定されていません。マテリアル変更はスキップします。");
                // マテリアルがない場合でもキャプチャを行う場合は、ここを調整
            }
            FrameObjectWithChildren(mainCamera, itemObject, 0.0001f, 1f); // アイテムをカメラの視野に収める

            // 強制的にシーンを再描画（マテリアル変更が反映されるように）
            SceneView.RepaintAll();
            // 少し待つことで描画が完了するのを助ける場合がありますが、Editorでは保証されません。
            // System.Threading.Thread.Sleep(50);

            // 2. カメラからの景色を Texture2D にして保存し、Scriptable Objectに代入。
            //    テクスチャーの名前をスクリプタブルオブジェクトと同じ名前にする。
            //    また指定のファイルにテクスチャーを保存する。

            // キャプチャ用のレンダーテクスチャーを作成
            RenderTexture captureRT = new RenderTexture(myScript.captureResolution.x, myScript.captureResolution.y, 24, RenderTextureFormat.ARGB32);

            mainCamera.targetTexture = captureRT; // カメラの出力先をキャプチャ用レンダーテクスチャーに設定

            // カメラをレンダリング
            mainCamera.Render();
            // RenderTexture の内容を Texture2D にコピー
            RenderTexture prevActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = captureRT;
            Texture2D screenShotTexture = new Texture2D(myScript.captureResolution.x, myScript.captureResolution.y, TextureFormat.ARGB32, false);

            screenShotTexture.ReadPixels(new Rect(0, 0, myScript.captureResolution.x, myScript.captureResolution.y), 0, 0);
            screenShotTexture.Apply(); // ReadPixels後にはApplyが必要

            Color[] pixels = screenShotTexture.GetPixels(); // 全ピクセルデータを取得

            for (int s = 0; s < pixels.Length; s++)
            {
                // ピクセル色と透過色の差を計算 (各RGB要素の差の絶対値の合計)
                float colorDifference = Mathf.Abs(pixels[s].r - transparentColor.r) +
                                        Mathf.Abs(pixels[s].g - transparentColor.g) +
                                        Mathf.Abs(pixels[s].b - transparentColor.b);

                // 差が許容誤差以下なら、そのピクセルを透明にする (アルファ値を0)
                if (colorDifference <= colorTolerance)
                {
                    pixels[s].a = 0f; // アルファ値を0に
                }
            }
            screenShotTexture.SetPixels(pixels); // 変更したピクセルデータを設定
            screenShotTexture.Apply(); // 変更をテクスチャに適用

            RenderTexture.active = prevActiveRenderTexture; // アクティブなRenderTextureを元に戻す
            // カメラの出力先を元に戻す
            mainCamera.targetTexture = originalCameraTargetTexture;
            mainCamera.clearFlags = prevClearFlags;
            mainCamera.backgroundColor = prevBackgroundColor;
            // キャプチャ用のレンダーテクスチャーを破棄
            DestroyImmediate(captureRT);

            // Texture2D を PNG データにエンコード
            byte[] bytes = screenShotTexture.EncodeToPNG();

            // 保存するファイルパスを決定
            string textureFileName = currentObjectData.name + ".png"; // Scriptable Objectと同じ名前に
            string assetPath = Path.Combine(fullSaveFolderPath, textureFileName);
            string filePath = Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""), textureFileName);

            // ファイルに書き出し
            File.WriteAllBytes(filePath, bytes);

            // Editor に新しいアセット（テクスチャー）を認識させる
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            Debug.Log($"テクスチャー '{textureFileName}' を '{assetPath}' に保存しました。");

            // 保存した Texture2D アセットをロード
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            // 3. Texture2D アセットを Scriptable Object の変数に代入し、Scriptable Object を保存。

            currentObjectData.icon = CreateSpriteFromScriptableObject(savedTexture);

            // Scriptable Object の変更をマーク
            EditorUtility.SetDirty(currentObjectData);

            // Scriptable Object の変更を保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); // Scriptable Object の変更も反映

            Debug.Log($"'{currentObjectData.name}' Scriptable Object に Texture2D への参照を保存しました。");

            // ランタイムの Texture2D はもう不要
            DestroyImmediate(screenShotTexture);
            // アイテムオブジェクトを破棄
            DestroyImmediate(itemObject);
        }

        // 処理後にシーンビューを元に戻した結果を反映
        SceneView.RepaintAll();

        Debug.Log("--- 全ての ObjectData (item data)の処理が完了しました (Texture2D 保存) ---");

         */
    }

    /// <summary>
    /// 指定されたオブジェクトとそのすべての子要素がカメラに収まるように調整します。
    /// Particle Systemを持つオブジェクトはサイズ計算から除外されます。
    /// </summary>
    /// <param name="camera">調整するカメラ</param>
    /// <param name="target">フレームに収めるターゲットの親オブジェクト</param>
    /// <param name="padding">オブジェクトと画面端との間の余白</param>
    /// <param name="zoomFactor">最終的な距離・サイズの倍率（1未満でズームイン）</param>
    public static void FrameObjectWithChildren(Camera camera, GameObject target, float padding = 0.1f, float zoomFactor = 1.0f)
    {
        // パーティクルを除外してバウンディングボックスを取得
        Bounds? totalBounds = GetTotalBoundsWithoutParticles(target);

        if (totalBounds == null)
        {
            Debug.LogWarning("ターゲットオブジェクトとその子要素に、表示可能なメッシュ等が見つかりませんでした。カメラの位置を調整できません。", target);
            return;
        }

        Bounds bounds = totalBounds.Value;

        // カメラの視野角（Field of View）
        float fov = camera.fieldOfView;

        // オブジェクトを収めるために必要な距離またはサイズを計算
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance;

        if (camera.orthographic)
        {
            // --- Orthographicカメラの場合 ---
            float requiredSizeX = bounds.size.x * 0.5f / camera.aspect;
            float requiredSizeY = bounds.size.y * 0.5f;
            camera.orthographicSize = Mathf.Max(requiredSizeX, requiredSizeY);

            // パディングとズームを適用
            camera.orthographicSize *= (1f + padding);
            camera.orthographicSize *= zoomFactor;
        }
        else
        {
            // --- Perspectiveカメラの場合 ---
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(fov * Mathf.Deg2Rad / 2f) * camera.aspect) * Mathf.Rad2Deg;
            float fovForCalc = (bounds.size.x / camera.aspect > bounds.size.y) ? horizontalFov : fov;

            distance = (objectSize * 0.5f) / Mathf.Tan(fovForCalc * 0.5f * Mathf.Deg2Rad);

            // パディングとズームを適用
            distance *= (1f + padding);
            distance *= zoomFactor;

            camera.transform.position = bounds.center - camera.transform.forward * distance;
        }

        // カメラの向きをオブジェクトの中心に向ける
        camera.transform.LookAt(bounds.center);
    }

    /// <summary>
    /// 指定されたオブジェクトとその子要素を包括するBoundsを計算します。
    /// ただし、ParticleSystemコンポーネントを持つオブジェクトは除外します。
    /// </summary>
    private static Bounds? GetTotalBoundsWithoutParticles(GameObject target)
    {
        // 親とすべての子からRendererコンポーネントを取得
        Renderer[] allRenderers = target.GetComponentsInChildren<Renderer>();

        // ParticleSystemを持たないRendererだけをフィルタリングする
        List<Renderer> filteredRenderers = new List<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer.GetComponent<ParticleSystem>() == null)
            {
                filteredRenderers.Add(renderer);
            }
        }

        // (参考) LINQを使った場合のより短い書き方
        // var filteredRenderers = allRenderers.Where(r => r.GetComponent<ParticleSystem>() == null).ToList();

        if (filteredRenderers.Count == 0)
        {
            return null; // 表示するメッシュなどがなければnullを返す
        }

        // 最初のRendererのBoundsを基準に設定
        Bounds totalBounds = filteredRenderers[0].bounds;

        // 2つ目以降のRendererのBoundsを結合していく
        for (int i = 1; i < filteredRenderers.Count; i++)
        {
            totalBounds.Encapsulate(filteredRenderers[i].bounds);
        }

        return totalBounds;
    }

    Sprite CreateSpriteFromScriptableObject(Texture2D textureContainer)
    {
        // 現在選択しているアセットを取得

        // 変換元のテクスチャを取得
        Texture2D sourceTexture = textureContainer;

        // 重要：テクスチャがSpriteとして使用可能か確認・設定変更
        string texturePath = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter != null)
        {
            // Texture Typeが「Sprite (2D and UI)」でない場合は変更を促す
            if (textureImporter.textureType != TextureImporterType.Sprite)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.SaveAndReimport(); // 設定を保存して再インポート
            }
        }

        // Spriteを作成
        // new Rect(x, y, width, height)
        Rect rect = new Rect(0, 0, sourceTexture.width, sourceTexture.height);
        // pivot (0.5, 0.5)で中央
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite newSprite = Sprite.Create(sourceTexture, rect, pivot);
        newSprite.name = $"{sourceTexture.name}_Sprite"; // Spriteオブジェクト自体の名前を設定

        // 保存先のパスを決定
        string soPath = AssetDatabase.GetAssetPath(textureContainer);
        string directory = Path.GetDirectoryName(soPath);
        // 元のテクスチャ名に「_sprite」を付けて、拡張子を「.asset」にする
        string savePath = Path.Combine(directory, $"{newSprite.name}.asset");
        // もし同名ファイルが存在した場合、Unityが自動でユニークなパスを生成してくれる
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

        // Spriteアセットをファイルとして保存
        AssetDatabase.CreateAsset(newSprite, uniquePath);
        AssetDatabase.SaveAssets(); // 保存を確定
        AssetDatabase.Refresh();    // Projectウィンドウを更新

        Debug.Log($"✅ Spriteアセットを作成しました: {uniquePath}", newSprite);

        // 生成されたアセットをProjectウィンドウでハイライト表示
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newSprite;
        return newSprite; // 作成したSpriteを返す
    }
void ProcessAllSphereData(ProcessMaterialData myScript)
    {
        if (myScript.transformToModify_Item == null)
        {
            Debug.LogError("Target Object To Modify が設定されていません。処理を中止します。");
            return;
        }

        if (myScript.sphereDataList == null || myScript.sphereDataList.Count == 0)
        {
            Debug.LogWarning("処理する  Data がリストにありません。");
            return;
        }

        // テクスチャー保存フォルダが存在するか確認し、なければ作成
        string fullSaveFolderPath = Path.Combine("Assets", myScript.textureSaveFolderPath.Replace("Assets/", ""));
        if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
        {
            if (Directory.Exists(Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""))))
            {
                // Folder exists in file system but not recognized by AssetDatabase
                AssetDatabase.Refresh();
            }
            else
            {
                // Create the folder if it doesn't exist
                string[] folders = myScript.textureSaveFolderPath.Replace("Assets/", "").Split('/');
                string currentPath = "Assets";
                foreach (string folder in folders)
                {
                    string newPath = Path.Combine(currentPath, folder);
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folder);
                    }
                    currentPath = newPath;
                }
                AssetDatabase.Refresh();
            }

            if (!AssetDatabase.IsValidFolder(fullSaveFolderPath))
            {
                Debug.LogError($"テクスチャー保存フォルダー '{fullSaveFolderPath}' の作成または確認に失敗しました。処理を中止します。");
                return;
            }
        }


        // 処理を開始する前に現在のマテリアルを保存しておく（必要であれば）
        // Material[] originalMaterials = targetRenderer.sharedMaterials;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("シーンに Main Camera がありません。テクスチャーの保存をスキップします。処理を中止します。");
            return;
        }

        // カメラの元のターゲットテクスチャーを保存しておく
        RenderTexture originalCameraTargetTexture = mainCamera.targetTexture;
        CameraClearFlags prevClearFlags = mainCamera.clearFlags;
        Color prevBackgroundColor = mainCamera.backgroundColor;

        Color transparentColor = Color.black; // 例として緑色を指定
        float colorTolerance = 0.01f;

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = transparentColor;

        // リストの各MaterialDataを処理
        for (int i = 0; i < myScript.sphereDataList.Count; i++)
        {
            SphereSpec sphereData = myScript.sphereDataList[i];
            if (sphereData == null)
            {
                Debug.LogWarning($"リストの要素 {i} がnullです。スキップします。");
                continue;
            }

            Debug.Log($"--- MaterialData 処理中 (Texture2D 保存): {sphereData.name} ({i + 1}/{myScript.catDataList.Count}) ---");

            // 1. オブジェクトをスクリプタブルオブジェクトから取得し変更する。
            GameObject itemObject = myScript.transformToModify_Item.gameObject;
            if (sphereData.Sphere != null)
            {
                itemObject = Instantiate(sphereData.Sphere, myScript.transformToModify_Item.position, myScript.transformToModify_Item.rotation);
            }
            else
            {
                Debug.LogWarning($"'{sphereData.name}' に Target prefab が設定されていません。マテリアル変更はスキップします。");
                // マテリアルがない場合でもキャプチャを行う場合は、ここを調整
            }
            FrameObjectWithChildren(mainCamera, itemObject, 0.0001f, 1f); // アイテムをカメラの視野に収める

            // 強制的にシーンを再描画（マテリアル変更が反映されるように）
            SceneView.RepaintAll();
            // 少し待つことで描画が完了するのを助ける場合がありますが、Editorでは保証されません。
            // System.Threading.Thread.Sleep(50);

            // 2. カメラからの景色を Texture2D にして保存し、Scriptable Objectに代入。
            //    テクスチャーの名前をスクリプタブルオブジェクトと同じ名前にする。
            //    また指定のファイルにテクスチャーを保存する。

            // キャプチャ用のレンダーテクスチャーを作成
            RenderTexture captureRT = new RenderTexture(myScript.captureResolution.x, myScript.captureResolution.y, 24, RenderTextureFormat.ARGB32);

            mainCamera.targetTexture = captureRT; // カメラの出力先をキャプチャ用レンダーテクスチャーに設定

            // カメラをレンダリング
            mainCamera.Render();
            // RenderTexture の内容を Texture2D にコピー
            RenderTexture prevActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = captureRT;
            Texture2D screenShotTexture = new Texture2D(myScript.captureResolution.x, myScript.captureResolution.y, TextureFormat.ARGB32, false);

            screenShotTexture.ReadPixels(new Rect(0, 0, myScript.captureResolution.x, myScript.captureResolution.y), 0, 0);
            screenShotTexture.Apply(); // ReadPixels後にはApplyが必要

            Color[] pixels = screenShotTexture.GetPixels(); // 全ピクセルデータを取得

            for (int s = 0; s < pixels.Length; s++)
            {
                // ピクセル色と透過色の差を計算 (各RGB要素の差の絶対値の合計)
                float colorDifference = Mathf.Abs(pixels[s].r - transparentColor.r) +
                                        Mathf.Abs(pixels[s].g - transparentColor.g) +
                                        Mathf.Abs(pixels[s].b - transparentColor.b);

                // 差が許容誤差以下なら、そのピクセルを透明にする (アルファ値を0)
                if (colorDifference <= colorTolerance)
                {
                    pixels[s].a = 0f; // アルファ値を0に
                }
            }
            screenShotTexture.SetPixels(pixels); // 変更したピクセルデータを設定
            screenShotTexture.Apply(); // 変更をテクスチャに適用

            RenderTexture.active = prevActiveRenderTexture; // アクティブなRenderTextureを元に戻す
            // カメラの出力先を元に戻す
            mainCamera.targetTexture = originalCameraTargetTexture;
            mainCamera.clearFlags = prevClearFlags;
            mainCamera.backgroundColor = prevBackgroundColor;
            // キャプチャ用のレンダーテクスチャーを破棄
            DestroyImmediate(captureRT);

            // Texture2D を PNG データにエンコード
            byte[] bytes = screenShotTexture.EncodeToPNG();

            // 保存するファイルパスを決定
            string textureFileName = sphereData.name + ".png"; // Scriptable Objectと同じ名前に
            string assetPath = Path.Combine(fullSaveFolderPath, textureFileName);
            string filePath = Path.Combine(Application.dataPath, myScript.textureSaveFolderPath.Replace("Assets/", ""), textureFileName);

            // ファイルに書き出し
            File.WriteAllBytes(filePath, bytes);

            // Editor に新しいアセット（テクスチャー）を認識させる
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            Debug.Log($"テクスチャー '{textureFileName}' を '{assetPath}' に保存しました。");

            // 保存した Texture2D アセットをロード
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            // 3. Texture2D アセットを Scriptable Object の変数に代入し、Scriptable Object を保存。

            sphereData.icon = CreateSpriteFromScriptableObject(savedTexture);

            // Scriptable Object の変更をマーク
            EditorUtility.SetDirty(sphereData);

            // Scriptable Object の変更を保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); // Scriptable Object の変更も反映

            Debug.Log($"'{sphereData.name}' Scriptable Object に Texture2D への参照を保存しました。");

            // ランタイムの Texture2D はもう不要
            DestroyImmediate(screenShotTexture);
            // アイテムオブジェクトを破棄
            DestroyImmediate(itemObject);
        }

        // 処理後にシーンビューを元に戻した結果を反映
        SceneView.RepaintAll();

        Debug.Log("--- 全ての ObjectData (item data)の処理が完了しました (Texture2D 保存) ---");
    }



}


