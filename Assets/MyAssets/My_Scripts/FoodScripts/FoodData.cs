using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using RotaryHeart.Lib.SerializableDictionary;

[CreateAssetMenu(fileName = "FoodData", menuName = "Scriptable Objects/FoodData")]
public class FoodData : ScriptableObject
{
    public string FoodName; //食べ物のタイトル
    public string Description; //食べ物の説明
    public Sprite foodIcon; //食べ物のアイコン

    public Cook_KitchenSystem.CookPattern cookPattern; //料理のパターン 現在なんの調理が行われている状態かを表す変数

[Serializable]
    public class cookDictionary : SerializableDictionaryBase<Cook_KitchenSystem.CookPattern, string> { }

[SerializeField]
    public cookDictionary cookPatternTitles; //料理のパターンごとのタイトルを格納する辞書

}
