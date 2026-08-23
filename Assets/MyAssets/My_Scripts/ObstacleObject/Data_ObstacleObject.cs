using UnityEngine;

[CreateAssetMenu(fileName = "Data_ObstacleObject", menuName = "Scriptable Objects/Data_ObstacleObject")]
public class Data_ObstacleObject : ScriptableObject
{
       [Tooltip("この障害物を壊すために必要な道具")]
    [SerializeField]
    public ToolType requiredTool = ToolType.None;
 [Tooltip("この障害物を破壊するために必要なタップ回数")]
    [SerializeField, Min(1)]
    public int requiredTapCount = 1;
}
