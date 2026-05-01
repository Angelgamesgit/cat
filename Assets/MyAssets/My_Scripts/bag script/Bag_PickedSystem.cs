using UnityEngine;

public class Bag_PickedSystem : MonoBehaviour
{
    bool picked = false; // アイテムが拾われたかどうかのフラグ
    // Start is called once before the first execution of Update after the MonoBehaviour is created
        void StartBagMiniGame(Bag_PickupItem pickedItem)
    {
        if (picked) return; // すでに拾われている場合は何もしない
        Bag_GameController.Instance.OpenBag(pickedItem.itemData); // バッグミニゲームを開始（アイテムデータは後で渡す）
        //Bag_GameController.Instance.OnBagClosed += HandleBagClosed; // バッグが閉じられたときのイベントを登録
    }
    void OnTriggerEnter(Collider other)
    {
        if (picked) return; // すでに拾われている場合は何もしない
        if (!other.CompareTag("Item")) return; // バッグピックアップアイテム以外は無視
            //UIをOnにする
        StartBagMiniGame(other.gameObject.GetComponent<Bag_PickupItem>());
            //UIにアイテムの情報を渡す
            //itempickedUI.GetComponent<ItemPickedUI>().SetItem(pickedItem.bagItemData);
            Destroy(other.gameObject); // アイテムを非表示にする
    }
    void HandleBagClosed(Bag_GameController.BagState state)
{
    Debug.Log("バッグ閉じた：" + state);
    picked = false; // アイテムが拾われていない状態に戻す
}
}
