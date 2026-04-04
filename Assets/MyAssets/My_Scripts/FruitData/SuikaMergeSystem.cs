using UnityEngine;

public class SuikaMergeSystem : MonoBehaviour
{
    public static SuikaMergeSystem Instance;

    public GameObject fruitPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void RequestMerge(SuikaFruit a, SuikaFruit b)
    {
        if (a.Data.next == null) return;

        a.LockMerge();
        b.LockMerge();

        Vector3 pos = (a.transform.position + b.transform.position) * 0.5f;

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        GameObject obj = Instantiate(fruitPrefab, pos, Quaternion.identity, transform);
        SuikaFruit newFruit = obj.GetComponent<SuikaFruit>();
        newFruit.Initialize(a.Data.next);

        SuikaScoreSystem.Instance.AddScore(a.Data.next.score);
    }
}