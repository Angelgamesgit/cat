using UnityEngine;

public class BagGameController : MonoBehaviour
{
    public enum BagResult
    {
        Success,
        Failed
    }

    public static BagGameController Instance;

    [Header("Systems")]
    public BagSpawner spawner;
    public BagGameOverLine overflowLine;

    [Header("Debug")]
    public bool playing;
    public BagResult result;

    [Header("Test")]
    public BagItemData[] testItem;

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

    public void OpenBag(BagItemData item)
    {
        Debug.Log("[BagGame] OpenBag : " + item.itemName);

        result = BagResult.Success;
        playing = true;

        gameObject.SetActive(true);

        spawner.TrySpawn(item);
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

        BagEventBridge.NotifyBagClosed(result);
    }
}