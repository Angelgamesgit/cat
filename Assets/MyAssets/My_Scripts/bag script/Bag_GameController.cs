using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using UnityEditor;
using Unity.Mathematics;

public class Bag_GameController : MonoBehaviour
{
    public enum BagState
    {
        Close, // 待機中
        Open, // バッグオープン中
    }

    public static Bag_GameController Instance;


    [Header("Debug")]

    public BagState state;

    [Header("Test")]
    public Bag_ItemData[] testItem;
    [SerializeField] GameObject bagUI;

    [SerializeField] GameObject overflowLine;
    [SerializeField] GameObject spawnItemPrefab;
    [SerializeField] Transform spawnTransform;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[BagGame] Controller Awake");
    }

    void Start()
    {

    }

    void Update()
    {
    }
//アイテムを拾うときに呼ばれる関数。Bag_ItemDataを渡すとバッグが開いてアイテムがスポーンする
    public void OpenBag(Bag_ItemData item)
    {
        Debug.Log("[BagGame] OpenBag : " + item.itemName);

        state = BagState.Open;
        bagUI.SetActive(true);
        GameObject obj = Instantiate(spawnItemPrefab,spawnTransform);
        obj.GetComponent<RectTransform>().anchoredPosition = spawnTransform.GetComponent<RectTransform>().anchoredPosition;
        obj.GetComponent<Bag_Item>().Initialize(item);
    }
//バッグがいっぱいになったときに呼ばれる関数。バッグオーバーフローの処理を行う　ラインのイベントから呼ばれる
    void Fail()
    {
        Debug.Log("[BagGame] BAG OVERFLOW");

        state = BagState.Close;
    }
//ボタンを押すと呼ばれる関数。バッグを閉じる処理を行う　バッグのUIから呼ばれる
    public void CloseBag()
    {
        Debug.Log("[BagGame] CloseBag State = " + state);

        state = BagState.Close;
        Bag_EventBridge.NotifyBagClosed(state);
        bagUI.SetActive(false);
    }
}