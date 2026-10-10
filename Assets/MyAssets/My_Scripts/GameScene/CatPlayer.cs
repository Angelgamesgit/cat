using UnityEngine;
public class CatPlayer : CatSystem
{
    public void MoveCatSet(GameSystem setSystem)
    {
        system = setSystem;
        targetObject = system.targetObject.transform;
        sphereObject = system.sphere.transform;
        AssignVariables();
    }

    private void OnTriggerEnter(UnityEngine.Collider other)
    {
        // 落とし物に触れたことをゲームシステムへ通知する。
        if (other.CompareTag("LostItem"))
        {
            system.LostItemTouched();
            Destroy(other.gameObject);
        }
if (other.CompareTag("RandomDebuffItem"))
        {
             // トラップまたはアイテムに付いた効果を発動する。
            RandomDebuffItem debuffItem = other.GetComponent<RandomDebuffItem>();
            debuffItem.OnPlayerTouched(this);
        }



        // 猫に触れたらゲームクリアを通知する。
        if (other.CompareTag("Cat"))
        {
            system.GameClear();
        }
    }
    public void DropCargo()
    {
        // 落とし物を落とす処理を実装する。
        // ここでは仮に落とし物の位置をリセットする例を示す。
        if (system != null && system.lostItemPrefab != null)
        {
            GameObject lostItem = Instantiate(system.lostItemPrefab, transform.position, Quaternion.identity);
            // 必要に応じて落とし物の初期化処理を追加する。
        }
    }
}
