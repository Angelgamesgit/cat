using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
public class StartMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool change;
[SerializeField]



public Vector3[] spherePos;
//移動する分の数字にマイナスかプラスをかける
[SerializeField]
 Transform[] sphere;
Dictionary<Transform, Vector3> spherePosDict;

public Transform currentSphere;

void Start()
    {
        change = false;
       
        spherePosDict = new Dictionary<Transform, Vector3>();
       
       
        spherePos = new Vector3[sphere.Length];
        for(int i = 0; i < sphere.Length; i++)
        {
            spherePos[i] = sphere[i].position;
            spherePosDict.Add(sphere[i], spherePos[i]);
        }
    }



public void OnSelectClick(int index)
{
    //演出
    StartCoroutine(SphereChange(index));
    currentSphere = sphere[index+ 1];
    //ゲームシステムなどに反映
}
   IEnumerator SphereChange(int Index)
    {
       Dictionary<Transform,Vector3> currentPosDict = new Dictionary<Transform, Vector3>();
        for(int i = 0; i < sphere.Length; i++)
        {
            currentPosDict.Add(sphere[i], sphere[i].position);
        }
        while(change)
        {
yield return null;
         }


change = true;
int maxcount = 10;
for(int c = 0; c <= maxcount ;c++)
{
for(int i = 0; i < sphere.Length; i++)
{
    sphere[i].position = Vector3.Lerp(currentPosDict[sphere[i]], spherePosDict[sphere[(i - Index + sphere.Length) % sphere.Length]], (float)c / maxcount);
}
    
yield return null;
}
change = false;
}
}
