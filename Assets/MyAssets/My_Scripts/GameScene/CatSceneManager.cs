using UnityEngine;
using UnityEngine.SceneManagement;
public class CatSceneManager : MonoBehaviour
{
  [SerializeField]
  UISystem uISystem; // UIシステムの参照
  public void currentSceneLoad()
  {
    Scene currentScene = SceneManager.GetActiveScene();
    uISystem.Load_Show(() => SceneManager.LoadSceneAsync(currentScene.name));
  }

  public void SceneChange_Garden()
  {
    uISystem.Load_Show(() => SceneManager.LoadSceneAsync("Stage_Garden"));
  }


  public void SceneChange_Cat()
  {

    uISystem.Load_Show(() => SceneManager.LoadSceneAsync("Stage"));
  }

}
