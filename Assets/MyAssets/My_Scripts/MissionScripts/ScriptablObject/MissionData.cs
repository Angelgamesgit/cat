using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Scriptable Objects/MissionData")]
public class MissionData : ScriptableObject
{
    public string Title; //ミッションのタイトル
    public string Description; //ミッションの説明
    public Sprite sprite; //ミッションのアイコン
}
