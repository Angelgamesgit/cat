using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FoodDeliveryUISystem : MonoBehaviour
{
    [SerializeField]
    Image foodData;
    [SerializeField]
    RectTransform getFoodPanel;
    FoodDeliverySystem foodDeliverySystem;
    [SerializeField]
    FoodData currentFoodData;

     void Start()
    {
        foodDeliverySystem = FindFirstObjectByType<FoodDeliverySystem>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void FoodIconChange(FoodData SetfoodData)
    {
        foodData.sprite = SetfoodData.foodIcon;
    }
    public void ShowFoodDeliveryUI(FoodData foodData)
    {
        UISystem.Instance.Panel_Open(getFoodPanel); // 食料配達UIを開く
        currentFoodData = foodData; // 現在の食料データを更新
    }

    public void HideFoodDeliveryUI()
    {
        UISystem.Instance.Panel_Close(getFoodPanel); // 食料配達UIを閉じる
    }
    public void SetDeliverFood()
    {
        FoodIconChange(currentFoodData); // 食料のアイコンを更新
        foodDeliverySystem.SetDeliverFood(currentFoodData); // 食料配達システムに現在の食料データを設定
    }
}
