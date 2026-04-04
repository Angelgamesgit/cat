using UnityEngine;

public class playercamera : MonoBehaviour
{
    //ターゲットにはネコもしくは目的地を設定する
   [SerializeField]
   Transform target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
    }
}
