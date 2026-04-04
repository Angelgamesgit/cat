using UnityEngine;

public class BagSystem : MonoBehaviour
{
  void UISystemOpen(Item item)
    {
        
    }
    void uisysbutton()
    {
    Spawn.getcomponent<SpriteRenderer>().sprite = item.itemSprite;
        //動かす
    }

    void otosu()
    {
        //アイテムを落とす
        Instantiate(item, spawner.transform.position, Quaternion.identity);
    }
    void uisysclose()
    {
        //UIをOffにする
        itempickedUI.SetActive(false);
    }

    UI関連はまとめてある;
}
