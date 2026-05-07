using UnityEngine;

public class FoodDeliverySystem : MonoBehaviour
{
    GameSystem gameSystem;
    [SerializeField]
    FoodDeliveryUISystem foodDeliveryUISystem;

    void Start()
    {
        foodDeliveryUISystem = GetComponent<FoodDeliveryUISystem>();
        gameSystem = FindFirstObjectByType<GameSystem>();
    }
    public void SetDeliverFood(FoodData SetfoodData)
    {
        gameSystem.playerData.ChangeCurrentFoodData(SetfoodData);
        Debug.Log("Delivering " + gameSystem.playerData.currentFoodData.name);
        foodDeliveryUISystem.FoodIconChange(gameSystem.playerData.currentFoodData);
    }
}
