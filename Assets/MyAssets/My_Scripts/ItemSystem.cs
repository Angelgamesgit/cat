using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemSystem : MonoBehaviour
{
    [SerializeField]
    Garden_ItemSO[] allGardenItems;
    [SerializeField]
    UISystem uISystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void newItemGet()
    {
        //85%の確率で設計図を手に入れる
        if (Random.Range(0f, 1f) < 0.85f)
        {
            //設計図を手に入れた
            Debug.Log("設計図を手に入れた");
            //手に入れていないガーデンアイテムを探し、リストにする
            List<Garden_ItemSO> gardenItems = new List<Garden_ItemSO>();
            foreach (Garden_ItemSO item in allGardenItems)
            {
                if (item.initialTotalPossession == 0)
                {
                    gardenItems.Add(item);
                }
            }
            if (gardenItems.Count != 0)
            {
                int r = Random.Range(0, gardenItems.Count);
                gardenItems[r].initialTotalPossession++;
                uISystem.NewGetItem_DirectionPlay(UISystem.AnimationPattern.Popup, gardenItems[r].icon, gardenItems[r].displayName);
            }
            else
            {
                Debug.Log("全てのガーデンアイテムを手に入れています。");
            }
           
        }
       // 交換券を手に入れる
    }
}
