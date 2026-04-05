using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Bag_ItemData itemData;

    bool picked;

    void OnTriggerEnter(Collider other)
    {
        if (picked) return;

        if (other.CompareTag("Player"))
        {
            picked = true;

            StartBagMiniGame();
        }
    }

    void StartBagMiniGame()
    {
        Bag_GameController.Instance.OpenBag(itemData);

        gameObject.SetActive(false);
    }
}