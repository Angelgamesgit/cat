using UnityEngine;
using Random = UnityEngine.Random;
public class MissionSystem : MonoBehaviour
{
    [SerializeField]
    MissionData[] missionDatas; //ミッションのデータを格納する配列
    public static MissionSystem missionSystem; //ミッション選択システムの情報を取得するための変数
    //ミッションを選択するシステムのスクリプト
    void Start()
    {
        missionSystem = this;
    }
    /// <summary>
    /// ミッションを選択するシステムの関数
    /// </summary>
    public void MissionSelect()
    {
        int randomIndex = Random.Range(0, missionDatas.Length); //ミッションのデータをランダムで選択する
        MissionData selectedMission = missionDatas[randomIndex];
        //選択されたミッションのデータを使用して、UIに表示するなどの処理を行う
        GetComponent<MissionUISystem>().ShowMissionUI(selectedMission);
    }
}
