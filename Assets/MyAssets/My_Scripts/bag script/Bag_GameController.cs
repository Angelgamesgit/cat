using UnityEngine;

public class Bag_GameController : MonoBehaviour
{
    public enum BagResult
    {
        Success,
        Failed
    }

    public static Bag_GameController Instance;

    [Header("Systems")]
    public Bag_Spawner spawner;
    public Bag_GameOverLine overflowLine;

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
        overflowLine.OnOverflow += Fail;
        Debug.Log("[BagGame] Start OK");
    }

    void Update()
    {
        // Editor / Android両方でテスト可能
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("[BagGame] Debug Spawn");

            OpenBag(testItem[Random.Range(0, testItem.Length)]);
        }
    }

    public void OpenBag(Bag_ItemData item)
    {
        Debug.Log("[BagGame] OpenBag : " + item.itemName);

        result = BagResult.Success;
        playing = true;

        bag2D.SetActive(true);
        spawner.TrySpawn(item);
        spawner.GetComponent<SpriteRenderer>().sprite = item.icon;
    }

    void Fail()
    {
        Debug.Log("[BagGame] BAG OVERFLOW");

        result = BagResult.Failed;

        CloseBag();
    }

    public void CloseBag()
    {
        Debug.Log("[BagGame] CloseBag Result = " + result);

        playing = false;

        Bag_EventBridge.NotifyBagClosed(result);
    }
}