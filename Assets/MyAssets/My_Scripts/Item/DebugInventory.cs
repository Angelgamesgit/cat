using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UI;
using DG.Tweening;


public class DebugInventory : MonoBehaviour
{
    public Item[] testItems;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            var data = testItems[UnityEngine.Random.Range(0, testItems.Length)];
            InventoryManager.I.SpawnItem(data);
        }
    }
}
