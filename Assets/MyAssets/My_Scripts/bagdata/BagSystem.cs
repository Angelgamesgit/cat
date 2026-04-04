using UnityEngine;

public class BagSystem : MonoBehaviour
{
    GameObject itempickedUI;
void UISystemOpen(BagItemData item)
    {

    }
    void uisysbutton(BagItemData item)
    {
        //スポナーの画像を変更して動かす
        //落とすときには別のオブジェクトを生成する
        GameObject spawner = GameObject.Find("Spawner");
        spawner.GetComponent<SpriteRenderer>().sprite = item.icon;
        //動かす
    }

    void otosu()
    {
        //アイテムを落とす
        GameObject items = Instantiate(item, spawner.transform.position, Quaternion.identity);
    }

    void uisysclose()
    {
        //UIをOffにする
        itempickedUI.SetActive(false);
    }
}
