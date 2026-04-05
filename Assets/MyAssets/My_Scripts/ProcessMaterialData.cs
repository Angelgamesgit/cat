using UnityEngine;
using System.Collections.Generic;

public class ProcessMaterialData : MonoBehaviour
{
    [Tooltip("マテリアルを変更する対象のオブジェクト")]
    public GameObject targetObjectToModify_Cat;

    public Transform transformToModify_Item;

    [Tooltip("処理する Scriptable Objectのリスト")]
    public List<CatData> catDataList;

    public List<SphereSpec> sphereDataList; // Sphere のデータ

    [Tooltip("カメラからの景色を保存するテクスチャーの解像度")]
    public Vector2Int captureResolution = new Vector2Int(1080, 1080);

    [Tooltip("テクスチャーを保存するAssets内のフォルダーパス (例: Assets/CapturedTextures)")]
    public string textureSaveFolderPath = "Assets/CapturedTextures";

    // このスクリプト自体はシーンにアタッチされますが、実際の処理はCustomEditorで行います。
}