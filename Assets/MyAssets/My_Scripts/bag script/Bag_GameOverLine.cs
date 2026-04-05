using UnityEngine;

public class Bag_GameOverLine : MonoBehaviour
{
    public float limitY;

    public System.Action OnOverflow;

    Bag_Item[] items;

    void Update()
    {
        items = FindObjectsOfType<Bag_Item>();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].transform.position.y > limitY)
            {
                Debug.Log("[BagGame] Overflow");

                OnOverflow?.Invoke();
                break;
            }
        }
    }
}