using UnityEngine;

[CreateAssetMenu(fileName = "CatData", menuName = "Scriptable Objects/CatData")]
public class CatData : ScriptableObject
{
      [Tooltip("猫ちゃんのマテリアル = 模様")]
    public Material targetMaterial;

    public Sprite catIcon;
}
