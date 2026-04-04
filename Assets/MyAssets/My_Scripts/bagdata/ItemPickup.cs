using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public BagItemData itemData;

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
        BagGameController.Instance.OpenBag(itemData);

        gameObject.SetActive(false);
    }
}