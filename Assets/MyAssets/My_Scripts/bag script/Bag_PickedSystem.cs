using UnityEngine;

public class Bag_PickedSystem : MonoBehaviour
{
    bool picked = false; // アイテムが拾われたかどうかのフラグ
    // Start is called once before the first execution of Update after the MonoBehaviour is created
        void StartBagMiniGame()
    {
        gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (picked) return;
        StartBagMiniGame();
        if (other.CompareTag("Item"))
        {
            //UIをOnにする
        Bag_GameController.Instance.OpenBag(other.gameObject.GetComponent<Bag_ItemData>());
            //UIにアイテムの情報を渡す
            //itempickedUI.GetComponent<ItemPickedUI>().SetItem(pickedItem.bagItemData);

        }
    }
    void HandleBagClosed(Bag_GameController.BagResult result)
{
    Debug.Log("バッグ閉じた：" + result);
    picked = false; // アイテムが拾われていない状態に戻す
}
}
