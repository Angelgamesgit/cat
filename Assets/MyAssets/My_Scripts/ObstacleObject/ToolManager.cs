using UnityEngine;

/// <summary>
/// プレイヤーが現在使用している道具を管理します。
/// </summary>
public class ToolManager : MonoBehaviour
{
    [Header("現在選択している道具")]

    [SerializeField]
    private ToolType currentTool = ToolType.None;

    /// <summary>
    /// 現在選択されている道具。
    /// </summary>
    public ToolType CurrentTool => currentTool;

    /// <summary>
    /// 道具を変更します。
    /// </summary>
    public void SelectTool(ToolType tool)
    {
        currentTool = tool;

        Debug.Log(
            $"現在の道具を {currentTool} に変更しました。"
        );
    }

    /// <summary>
    /// 道具を解除します。
    /// </summary>
    public void ClearTool()
    {
        currentTool = ToolType.None;
    }
}