using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/CombineDatabase")]
public class CombineDatabase : ScriptableObject
{
    public List<CombinePair> pairs;

    public Item GetResult(Item a, Item b)
    {
        foreach (var p in pairs)
        {
            if ((p.a == a && p.b == b) || (p.a == b && p.b == a))
                return p.result;
        }
        return null;
    }
}

[System.Serializable]
public class CombinePair
{
    public Item a;
    public Item b;
    public Item result;
}
