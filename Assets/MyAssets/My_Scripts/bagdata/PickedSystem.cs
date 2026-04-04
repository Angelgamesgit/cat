using UnityEngine;

public class PickedSystem : MonoBehaviour
{
    [SerializeField] private GameObject itempickedUI; // UIのプレハブ(取得)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            PickedItem pickedItem = other.GetComponent<PickedItem>();
            //UIをOnにする
itempickedUI.SetActive(true);
            //UIにアイテムの情報を渡す
            itempickedUI.GetComponent<ItemPickedUI>().SetItem(pickedItem.bagItemData);

        }
    }
    
}
