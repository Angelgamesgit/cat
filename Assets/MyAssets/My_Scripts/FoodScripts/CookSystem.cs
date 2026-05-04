using UnityEngine;

public class CookSystem : MonoBehaviour
{//料理システムのスクリプト
    FoodData[] foodDatas; //食べ物のデータを格納する配列
    public  enum CookPattern
    {
        //料理のパターンを定義する列挙型
        none, //未調理
        fire,//焼く
        boil, //煮る
        steam, //蒸す
        fry, //揚げる
        cut, //切る
    }
    public void Cook(CookPattern cookPattern)
    {
        //料理をする処理をここに記述
        //例: 食べ物のデータを使用して、料理の結果をUIに表示するなどの処理を行う
        switch (cookPattern)
        {
            case CookPattern.fire:
                //焼く処理をここに記述
                break;
            case CookPattern.boil:
                //煮る処理をここに記述
                break;
            case CookPattern.steam:
                //蒸す処理をここに記述
                break;
            case CookPattern.fry:
                //揚げる処理をここに記述
                break;
            case CookPattern.cut:
                //切る処理をここに記述
                break;
        }
    }
}
