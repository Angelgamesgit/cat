
public class CatPlayer : CatSystem
{   public void MoveCatSet(GameSystem setSystem)
    {
        system = setSystem;
        targetObject = system.targetObject.transform;
        sphereObject = system.sphere.transform;
        AssignVariables();
    }

    void OnTriggerEnter(UnityEngine.Collider other)
    {
        //ロストアイテムに触れたら、ゲームシステム側に触れたことを伝える
        if (other.CompareTag("LostItem"))
        {
            system.LostItemTouched();
            Destroy(other.gameObject);
        }
        if(other.CompareTag("Cat"))
        {
            system.GameEnd();
        }
    }
}
