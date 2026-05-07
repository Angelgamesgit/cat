using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    public string Title; //ミッションのタイトル
    public string Description; //ミッションの説明
    public Sprite foodSprite; //ミッションの食料アイコン
    public FoodData foodData; //ミッションの食料データ
    public CookSystem.CookPattern cookPattern; //ミッションの大成功料理のパターン


}
