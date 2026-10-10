using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    private GameSystem gameSystem;
    private PlayerData playerData;
    private LightResourceSystem lightResourceSystem;
    private bool ending;

    public void Initialize(GameSystem owner, PlayerData data, LightResourceSystem lightSystem)
    {
        gameSystem = owner;
        playerData = data;
        lightResourceSystem = lightSystem;
    }

    public void BeginGame()
    {
        ending = false;
        if (gameSystem != null) gameSystem.isPlaying = true;
    }

    // 落とし物取得時に猫を出現させる処理をまとめる。
    public void HandleLostItemFound()
    {
        if (gameSystem == null || gameSystem.findCatObject == null) return;
        gameSystem.find_LostItem = true;
        gameSystem.findCatObject.SetActive(true);
        Debug.Log("Lost item collected. The cat has appeared.");
    }

    // 猫がゴールに触れたときのクリア処理をまとめる。
    public void HandleGameClear()
    {
        if (ending) return;
        ending = true;
        if (playerData != null) playerData.Save();
        if (gameSystem != null) gameSystem.isPlaying = false;
        CatSceneManager.Instantiate().currentSceneLoad();
    }

    // 光切れかつ猫が停止した状態をゲーム終了として扱う。
    public void CheckGameEnd()
    {
        if (ending || gameSystem == null || !gameSystem.isPlaying || lightResourceSystem == null || gameSystem.PlayerCat == null) return;
        if (!lightResourceSystem.IsDepleted || gameSystem.PlayerCat.currentState != CatPlayer.AnimState.idle) return;
        ending = true;
        Debug.Log("Game over condition reached.");
        if (playerData != null) playerData.Save();
        gameSystem.isPlaying = false;
        CatSceneManager.Instantiate().currentSceneLoad();
    }
}
