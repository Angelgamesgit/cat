using UnityEngine;

public class BagGameOverLine : MonoBehaviour
{
    public float limitY;

    public System.Action OnOverflow;

    BagItem[] items;

    void Update()
    {
        items = FindObjectsOfType<BagItem>();

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