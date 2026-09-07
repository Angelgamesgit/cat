using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Dictionaryへの変換で利用
public class DaySphereSystem : MonoBehaviour
{
  
    [Serializable]
    public class DaySphere
    {
        public DaySphereType type;
        public GameObject sphereObject;
    }

    public List<DaySphere> daySpheres = new List<DaySphere>();
    
    public enum DaySphereType
    {
        Day,
        Night,
        Dusk,
        Dawn,
        Rain,
        Snow
    }
    Dictionary<DaySphereType, GameObject> sphereDic;
    public void SphreSet(DaySphereType type)
    {
sphereDic = new Dictionary<DaySphereType, GameObject>();

        sphereDic = daySpheres.ToDictionary(set => set.type, set => set.sphereObject);
        sphereDic[type].SetActive(true);
        //他のSphereを非表示にする
        foreach (var sphere in sphereDic)
        {
            if (sphere.Key != type)
            {
                sphere.Value.SetActive(false);
            }
        }

    }
    void Update()
    {
        //空の球体をゆっくり回転させる
        sphereDic.FirstOrDefault(x => x.Value.activeSelf).Value.transform.Rotate(new Vector3 (1,1,0) * 0.01f);
    }
}
