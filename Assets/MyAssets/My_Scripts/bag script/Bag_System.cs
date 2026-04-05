using UnityEngine;

public class Bag_System : MonoBehaviour
{
    GameObject itempickedUI;
    GameObject item;
    GameObject spawner;
void UISystemOpen(Bag_ItemData item)
    {

    }
    void uisysbutton(Bag_ItemData item)
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
