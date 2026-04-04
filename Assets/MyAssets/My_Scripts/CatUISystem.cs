using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using DG.Tweening;
using TMPro;

public class CatUISystem : MonoBehaviour
{
    [SerializeField]
    Image[] catIcons;

    [SerializeField]
    Image sphereIcon;

    [SerializeField]
    GameObject pages;
    [SerializeField]
    Sprite questionMarkIcon; // クエスチョンマークのアイコン
    [HideInInspector]
    public SphereSpec sphereSpec;
    [SerializeField]
    GameSystem system;

    int currentIndex;
    [SerializeField]
    Button stageChangeButton;

    bool isanimating;

    UISystem uiSystem;
    [SerializeField]
TextMeshProUGUI sphereNameText;
    
    [Header("UI Indicators")]
    [SerializeField]
    private GameObject indicatorContainer; // インジケーターの親オブジェクト（Layout Group設定推奨）
    [SerializeField]
    private GameObject indicatorPrefab; // インジケーターとして使うImageのプレハブ
    [SerializeField]
    private Color activeIndicatorColor = Color.white; // 現在のページを示す色
    [SerializeField]
    private Color inactiveIndicatorColor = Color.gray; // それ以外のページの色

    private List<GameObject> indicatorImages = new List<GameObject>(); // 生成したインジケーターを保持するリスト

    void Start()
    {
        uiSystem = GetComponent<UISystem>();
        isanimating = false;
    }

    void Update()
    {
        if (pages.activeSelf)
        {
            pagesFlick();
        }
    }
    // マウスドラッグ開始・終了座標を保持する変数を追加
    Vector2 mouseDownPos;
    Vector2 mouseUpPos;

    void pagesFlick()
    {
        float minFlickDistance = 50f;
        // タッチ操作
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                mouseDownPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                mouseUpPos = touch.position;
                float flickDistance = mouseUpPos.x - mouseDownPos.x;

                if (flickDistance > minFlickDistance)
                {
                    Previous();
                }
                else if (flickDistance < -minFlickDistance)
                {
                    Next();
                }
            }
        }
        // マウス操作
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseDownPos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                mouseUpPos = Input.mousePosition;
                float flickDistance = mouseUpPos.x - mouseDownPos.x;

                if (flickDistance > minFlickDistance)
                {
                    Previous();
                }
                else if (flickDistance < -minFlickDistance)
                {
                    Next();
                }
            }
        }
    }



    public void PaperSet(SphereSpec spec)
    {
        sphereSpec = spec;
        currentIndex = system.playerData.FindSphereList.IndexOf(sphereSpec);
        if (sphereSpec.icon != null)
        {
            sphereIcon.sprite = sphereSpec.icon;
        }

        for (int i = 0; i < catIcons.Length; i++)
        {

            if (spec.findCatsDate != null && i < spec.findCatsDate.Count && system.playerData.catFound.Contains(spec.findCatsDate[i]))
            {
                catIcons[i].sprite = spec.findCatsDate[i].catIcon;
            }
            else
            {
                catIcons[i].sprite = questionMarkIcon; // クエスチョンマークのアイコンを設定
            }
        }
        stageChangeButton.onClick.RemoveAllListeners();
        stageChangeButton.onClick.AddListener(() => SphereChange());
        sphereNameText.text = spec.sphereName;
        // インジケーターの更新
        UpdateIndicators();
    }
    
    
    void UpdateIndicators()
    {
        // プレハブ等が設定されていなければ処理を中断
        if (indicatorContainer == null || indicatorPrefab == null) return;

        int totalPages = system.playerData.FindSphereList.Count;

        // 現在のインジケーターの数とページの総数が違う場合、インジケーターを再生成
        if (indicatorImages.Count != totalPages)
        {
            // 既存のインジケーターをすべて削除
            foreach (GameObject img in indicatorImages)
            {
                Destroy(img.gameObject);
            }
            indicatorImages.Clear();

            // 必要な数だけインジケーターを生成
            for (int i = 0; i < totalPages; i++)
            {
                GameObject newIndicator = Instantiate(indicatorPrefab, indicatorContainer.transform);
                indicatorImages.Add(newIndicator);
            }
        }

        // すべてのインジケーターの色を更新
        for (int i = 0; i < indicatorImages.Count; i++)
        {
            // 現在のページに対応するインジケーターだけをアクティブカラーに設定
            indicatorImages[i].GetComponent<Image>().color = (i == currentIndex) ? activeIndicatorColor : inactiveIndicatorColor;
        }
    }
    


    // ページの中央座標を取得
    Vector3 GetCenterPosition()
    {
        return new Vector3(0, pages.transform.localPosition.y, 0); // 画面中央 (Canvasの中央)
    }

    // ページの右外座標を取得
    Vector3 GetRightOffscreenPosition()
    {
        return new Vector3(Screen.width, pages.transform.localPosition.y, 0);
    }

    // ページの左外座標を取得
    Vector3 GetLeftOffscreenPosition()
    {
        return new Vector3(-Screen.width, pages.transform.localPosition.y, 0);
    }

    public void Next()
    {
        if (isanimating) return; // アニメーション中はボタンを無効化
        isanimating = true;
        // Unity側でFindSphereListが空の場合にエラーが出るため、要素数をチェック
        if (system.playerData.FindSphereList == null || system.playerData.FindSphereList.Count == 0)
        {
            Debug.LogWarning("FindSphereList is empty. Cannot go to next page.");
            return;
        }
        int nextIndex = (currentIndex + 1) % system.playerData.FindSphereList.Count;
        // 現在のページを左外へスライド
        pages.transform.DOLocalMove(GetLeftOffscreenPosition(), 0.3f).OnComplete(() =>
        {
            // 次のデータをセット
            sphereSpec = system.playerData.FindSphereList[nextIndex];
            PaperSet(sphereSpec);
            // 新しいページを右外に配置
            pages.transform.DOLocalMove(GetRightOffscreenPosition(), 0f).OnComplete(() =>
            {
                // ページを右外から中央へスライドイン
                pages.transform.DOLocalMove(GetCenterPosition(), 0.3f).OnComplete(() =>
                {
                    isanimating = false;
                }); // アニメーション完了後にフラグをリセット
            });
            // 右外から中央へスライドイン
        });
    }

    public void Previous()
    {
        if (isanimating) return; // アニメーション中はボタンを無効化
        isanimating = true;
        // Unity側でFindSphereListが空の場合にエラーが出るため、要素数をチェック
        if (system.playerData.FindSphereList == null || system.playerData.FindSphereList.Count == 0)
        {
            Debug.LogWarning("FindSphereList is empty. Cannot go to previous page.");
            return;
        }
        int prevIndex = (currentIndex - 1 + system.playerData.FindSphereList.Count) % system.playerData.FindSphereList.Count;
        // 現在のページを右外へスライド
        pages.transform.DOLocalMove(GetRightOffscreenPosition(), 0.3f).OnComplete(() =>
        {
            // 前のデータをセット
            sphereSpec = system.playerData.FindSphereList[prevIndex];
            PaperSet(sphereSpec);

            // 新しいページを左外に配置
            pages.transform.DOLocalMove(GetLeftOffscreenPosition(), 0f).OnComplete(() =>
            {
                // 左外から中央へスライドイン
                pages.transform.DOLocalMove(GetCenterPosition(), 0.3f).OnComplete(() =>
                {
                    isanimating = false;
                }); // アニメーション完了後にフラグをリセット
            });
        });
    }

    void SphereChange()
    {
        if (uiSystem == null) return;
        if (sphereSpec == null) return;
        uiSystem.Load_Show(() =>
            {
                system.SphereSet(sphereSpec);
                uiSystem.Hide();
            });
    }
}
