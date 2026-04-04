using UnityEngine;

public class GroundsoundType : MonoBehaviour
{
    //鳴らす音の情報を保持
  public  SurfaceType type;
  
    void Start()
    {
        if (GetComponent<Collider>()) return;
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
    }
}
