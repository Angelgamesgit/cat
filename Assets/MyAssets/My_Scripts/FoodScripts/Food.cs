using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    FoodData foodData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetFood(GameSystem gameSystem)
    {
        Debug.Log("Player has touched the food!");
       
        gameSystem.foodDeliveryUISystem.ShowFoodDeliveryUI(foodData); // プレイヤーのSetDeliverFoodメソッドを呼び出して、食べ物のデータを渡す
        // プレイヤーが食べ物に触れたときの処理をここに記述
        // 例: 食べ物を消す、プレイヤーのステータスを回復するなど
        Destroy(gameObject); // 食べ物を消す例
    }
}
