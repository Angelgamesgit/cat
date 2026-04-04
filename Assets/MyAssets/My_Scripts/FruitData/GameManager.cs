using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject fruitPrefab;
    public FruitData[] fruitList;

    private void Awake()
    {
        Instance = this;
    }

    public FruitData GetRandomFruit()
    {
        return fruitList[UnityEngine.Random.Range(0, fruitList.Length)];
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }
}