using UnityEngine;

public class FindCat : CatSystem
{
    
    public void StartSet(GameSystem setsystem)
    {
        system = setsystem;
        targetObject = Instantiate(new GameObject(),transform.position, Quaternion.identity).transform;
        sphereObject = system.sphere.transform;
        targetObject.SetParent(transform);
        CatData data = system.findCatData;
    }
    public override void Update()
    {
        
    }
    void OnCollisionStay(Collision collision)
    {
        system.FindCatSet();
        DestroyImmediate(gameObject);
    }
}
