using UnityEngine;

public class Bag_GameController : MonoBehaviour
{
    public enum BagResult
    {
        Success,
        Failed
    }

    public static Bag_GameController Instance;


    [Header("Debug")]
    public bool playing;
    public BagResult result;

    [Header("Test")]
    public Bag_ItemData[] testItem;
    public GameObject bag2D;

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
        // overflowLine.OnOverflow += Fail;
        Debug.Log("[BagGame] Start OK");
    }

    void Update()
    {
        // Editor ＆ Android両方でテスト可能
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[BagGame] Debug Spawn");
            OpenBag(testItem[Random.Range(0, testItem.Length)]);
        }
    }
//アイテムを拾うときに呼ばれる関数。Bag_ItemDataを渡すとバッグが開いてアイテムがスポーンする
    public void OpenBag(Bag_ItemData item)
    {
        Debug.Log("[BagGame] OpenBag : " + item.itemName);

        result = BagResult.Success;
        playing = true;

        bag2D.SetActive(true);
        Debug.Log("未着手の箇所です アイテムのスポーン処理を実装してください");

    }
//バッグがいっぱいになったときに呼ばれる関数。バッグオーバーフローの処理を行う　ラインのイベントから呼ばれる
    void Fail()
    {
        Debug.Log("[BagGame] BAG OVERFLOW");

        result = BagResult.Failed;
    }
//ボタンを押すと呼ばれる関数。バッグを閉じる処理を行う　バッグのUIから呼ばれる
    public void CloseBag()
    {
        Debug.Log("[BagGame] CloseBag Result = " + result);

        playing = false;
        Bag_EventBridge.NotifyBagClosed(result);
    }
}