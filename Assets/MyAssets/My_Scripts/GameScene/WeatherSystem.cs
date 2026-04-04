using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq; // Dictionaryへの変換で利用


public class WeatherSystem : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Serializable]
    public class WeatherClass
    {
        public SphereSpec.WeatherState setState;
        public GameObject weatherObject;
    }

    public WeatherClass[] weatherClass;
    Dictionary<SphereSpec.WeatherState, GameObject> weatherDic;

    public  void WeatherSet(SphereSpec.WeatherState weatherState)
    {
        weatherDic = weatherClass.ToDictionary(set => set.setState, set => set.weatherObject);

        if (!weatherDic[weatherState]) return;

        foreach (GameObject obj in weatherDic.Values)
        {
            if (!obj) continue;
            obj.SetActive(false);
        }
        weatherDic[weatherState].SetActive(true);

    }


}
