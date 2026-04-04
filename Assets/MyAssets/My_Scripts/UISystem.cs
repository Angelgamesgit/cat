using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System;
using TMPro;
using Random = UnityEngine.Random;

public class UISystem : MonoBehaviour
{
    [SerializeField]
    GameSystem gameSystem;

    [SerializeField]
    GameObject SpherChangePanel;
    [SerializeField]
    GameObject SphereCanvas;

    [SerializeField]
    PlayerData playerData;

    [SerializeField]
    GameObject canvasRing;

    Vector2 startTouchPos;
    Vector2 endTouchPos;

    [SerializeField]
    GameObject ScneneChangePanel;

    //発見した猫関連

    public Image darkFadeImage;
    public GameObject itemGetPanel;
   public GameObject KitichenButton;


    void Start()
    {
        Hide();
    }
    // Update is called once per frame
    void Update()
    {
        RingRotate();
    }

    public void SphereChangeOpen()
    {
        SpherChangePanel.SetActive(true);
        canvasRing = Instantiate(new GameObject());
        canvasRing.name = "CanvasRing";
        canvasRing.transform.position = new Vector3(0, 0, 0);
        float radius = gameSystem.sphere.transform.localScale.x * 0.7f; // スフィアの半径

        int count = playerData.FindSphereList.Count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 position = new Vector3(x, 0, z);
            Quaternion rotation = Quaternion.LookRotation(position); // 中心から外向きに回転
            GameObject obj = Instantiate(SphereCanvas, position, rotation);
            obj.transform.SetParent(canvasRing.transform);
            obj.GetComponent<SphereCanvasButton>().SetStart(playerData.FindSphereList[i]);
            obj.transform.LookAt(gameSystem.mainCamera.transform);
        }

        canvasRing.transform.rotation = quaternion.identity;
        canvasRing.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        OpenAnimation();

    }
    void OpenAnimation()
    {
        canvasRing.transform.DOLocalRotate(new Vector3(-15, 720, 0), 3.5f, RotateMode.FastBeyond360);
        canvasRing.transform.DOScale(new Vector3(1, 1, 1), 2.25f).SetEase(Ease.OutBack);
        canvasRing.transform.DOMove(new Vector3(0, 5, 0), 2.25f).SetEase(Ease.OutBack);
    }

    public void SphereChangeClose()
    {
        SpherChangePanel.SetActive(false);
        DestroyImmediate(canvasRing);
        canvasRing = null;
        gameSystem.cameraPosChange(false, 0.3f);
    }
    void RingRotate()
    {
        if (canvasRing == null) return;
        // タッチ入力を検出
        if (Input.touchCount > 0)
        {
            //BurstImageAnimation burstImage = GetComponent<BurstImageAnimation>();
            //burstImage.CreatePairedBurstEffect();
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
            {
                foreach (Transform child in canvasRing.transform)
                {
                    child.transform.rotation = Camera.main.transform.rotation;
                }
                endTouchPos = touch.position;
                Vector2 delta = endTouchPos - startTouchPos;

                float rotationSpeed = 0.05f;  // 回転の強さ（感度）

                float rotationX = -delta.x * rotationSpeed;
                canvasRing.transform.Rotate(0, rotationX, 0, Space.Self);
                startTouchPos = touch.position;
            }
        }
    }

    public void SphereChange()
    {
        StartCoroutine(SphereChangeAnimation(gameSystem.playerData.currentSphereSpec));
    }

    public IEnumerator SphereChangeAnimation(SphereSpec sphereSpec)
    {
        //画面外からロケットが飛んできて中心から暗転
        ScneneChangePanel.SetActive(true);
        RectTransform iconRect = ScneneChangePanel.transform.Find("CenterIcon").GetComponent<RectTransform>();
        iconRect.position = new Vector3(Screen.width * -0.6f, Screen.height * -0.6f, 0);

        RectTransform blackPanel = ScneneChangePanel.transform.Find("BlackPanel").GetComponent<RectTransform>();
        blackPanel.localScale = new Vector2(0, 0);

        iconRect.DOAnchorPos(new Vector2(0, 0), 1.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(1.6f);

        blackPanel.DOScale(new Vector2(1, 1), 1.5f).SetEase(Ease.OutBack);

        //暗転は完了したのでシーンを変更するかシーンを読み込むか　専用の動かし方をするか
        //一旦専用の動かし方
        yield return new WaitForSeconds(4f);
        gameSystem.SphereSet(sphereSpec);
        blackPanel.DOScale(new Vector2(0, 0), 0.5f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.45f);
        iconRect.DOAnchorPos(new Vector2(Screen.width * 0.6f, Screen.height * 0.6f), 1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(1f);
        ScneneChangePanel.SetActive(false);

    }


    #region アイテム関連

    [Header("UI Elements")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI messageText; // 「ITEM GET!」などのメッセージ用

    [Header("Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.5f; // アニメーション時間
    [SerializeField] private Ease easeType = Ease.OutBack; // イージングの種類

    private RectTransform iconRect;
    private RectTransform itemNameRect;
    private RectTransform messageRect;

    private Vector2 originalIconSize; // アイコンの元のサイズを保持


    [SerializeField]
    GameObject notificationPrefab;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="patternIndex"></param>
    /// <param name="itemSprite"></param>
    /// <param name="itemName"></param>
    public enum AnimationPattern
    {
        Simple,
        Dynamic,
        Popup,
        Blueprint
    }
    public void NewGetItem_DirectionPlay(AnimationPattern patternIndex, Sprite itemSprite, string itemName)
    {
        iconRect = itemIcon.GetComponent<RectTransform>();
        itemNameRect = itemNameText.GetComponent<RectTransform>();
        messageRect = messageText.GetComponent<RectTransform>();
        originalIconSize = iconRect.sizeDelta;

        // アイテムの情報を設定
        itemGetPanel.SetActive(true);
        itemIcon.sprite = itemSprite;
        itemNameText.text = itemName;
        gameObject.SetActive(true);

        switch (patternIndex)
        {
            case AnimationPattern.Simple:
                PlaySimplePattern();
                break;
            case AnimationPattern.Dynamic:
                PlayDynamicPattern();
                break;
            case AnimationPattern.Popup:
                PlayPopupPattern();
                break;
            case AnimationPattern.Blueprint:
                PlayBlueprintPattern();
                break;
            default:
                PlaySimplePattern();
                break;
        }
    }

    // --- 演出パターン ---

    /// <summary>
    /// パターン1：シンプルに中央で表示
    /// </summary>
    private void PlaySimplePattern()
    {
        // 初期状態を設定
        iconRect.localScale = Vector3.zero;
        itemNameText.alpha = 0f;
        messageText.alpha = 0f;
        messageRect.anchoredPosition = new Vector2(0, -150f);

        // シーケンスを作成
        Sequence seq = DOTween.Sequence();

        seq.Append(iconRect.DOScale(1f, duration).SetEase(easeType)) // アイコンが拡大
           .AppendCallback(() =>
           {
               // 爆発エフェクトをアイコンの位置に生成
               if (explosionEffectPrefab != null)
               {
                   Instantiate(explosionEffectPrefab, iconRect.position, Quaternion.identity, transform);
               }
           })
           .Append(itemNameText.DOFade(1f, duration / 2)) // アイテム名がフェードイン
           .Join(messageText.DOFade(1f, duration / 2))     // メッセージがフェードイン
           .Join(messageRect.DOAnchorPosY(-100f, duration / 2)) // メッセージが少し上に移動
           .AppendInterval(1.5f) // 1.5秒待機
           .Append(GetComponent<CanvasGroup>().DOFade(0, duration)) // 全体をフェードアウト
           .OnComplete(() => gameObject.SetActive(false)); // 終わったら非表示
    }

    /// <summary>
    /// パターン2：ダイナミックに飛び込んでくる
    /// </summary>
    private void PlayDynamicPattern()
    {
        // 初期状態
        iconRect.anchoredPosition = new Vector2(-800f, 0); // 画面左外
        iconRect.localRotation = Quaternion.Euler(0, 0, -90f);
        itemNameText.alpha = 0f;
        messageText.alpha = 0f;

        // シーケンスを作成
        Sequence seq = DOTween.Sequence();

        seq.Append(iconRect.DOAnchorPosX(0, duration).SetEase(Ease.OutCubic)) // アイコンが左から中央へ
           .Join(iconRect.DORotate(Vector3.zero, duration).SetEase(Ease.OutCubic)) // 回転しながら
           .AppendCallback(() =>
           {
               if (explosionEffectPrefab != null)
               {
                   Instantiate(explosionEffectPrefab, iconRect.position, Quaternion.identity, transform);
               }
               // 衝撃で揺れる感じ
               transform.DOShakePosition(0.3f, new Vector3(10, 10, 0), 20, 90);
           })
           .Append(itemNameText.DOFade(1f, duration)) // アイテム名表示
                                                      //.Join(messageText.DOText("AWESOME!", duration, scrambleMode: ScrambleMode.All)) // 文字がガチャガチャしながら表示
           .AppendInterval(1.5f)
           .Append(iconRect.DOAnchorPosY(800f, duration).SetEase(Ease.InCubic)) // 上に飛んで消える
           .Join(itemNameText.DOFade(0f, duration))
           .Join(messageText.DOFade(0f, duration))
           .OnComplete(() => gameObject.SetActive(false));
    }


    /// <summary>
    /// パターン3：アイコンが弾けてテキストが出現
    /// </summary>
    private void PlayPopupPattern()
    {
        // 初期状態
        iconRect.localScale = Vector3.one * 2.5f; // 最初は大きい
        itemIcon.color = new Color(1, 1, 1, 0); // 透明
        itemNameText.alpha = 0;
        messageText.alpha = 0;

        // シーケンスを作成
        Sequence seq = DOTween.Sequence();

        seq.Append(itemIcon.DOFade(1f, 0.1f)) // パッと出現
           .Append(iconRect.DOScale(1f, duration).SetEase(Ease.OutBounce)) // バウンドしながら元のサイズに
           .AppendCallback(() =>
           {
               if (explosionEffectPrefab != null)
               {
                   Instantiate(explosionEffectPrefab, iconRect.position, Quaternion.identity, transform);
               }
           })
           .Append(iconRect.DOShakeScale(duration, 0.5f, 10, 90)) // 爆発に合わせてアイコンが揺れる
           .Append(itemNameText.DOFade(1f, duration))
           .Join(messageText.DOFade(1f, duration))
           .AppendInterval(1.5f)
           .Append(transform.DOScale(0, duration).SetEase(Ease.InBack)) // 全体が縮小して消える
           .OnComplete(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// パターン4：設計図が展開される演出 
    /// </summary>
    private void PlayBlueprintPattern()
    {
        // 初期状態を設定
        iconRect.localScale = Vector3.one;
        iconRect.pivot = new Vector2(0, 0.5f); // 左端を軸に展開するためPivotを変更
        iconRect.sizeDelta = new Vector2(0, originalIconSize.y); // 幅を0にしておく
        itemNameText.alpha = 0f;
        messageText.alpha = 0f;
        messageText.text = "設計図GET!";

        // シーケンスを作成
        Sequence seq = DOTween.Sequence();

        seq.Append(iconRect.DOSizeDelta(originalIconSize, duration).SetEase(Ease.OutCubic)) // 紙が横に広がる
           .AppendCallback(() =>
           {
               // 爆発エフェクトをアイコンの中央に生成
               if (explosionEffectPrefab != null)
               {
                   // Pivotを変更しているので、ワールド座標で中央を計算して生成
                   Vector3 centerPos = iconRect.TransformPoint(new Vector2(originalIconSize.x / 2, 0));
                   Instantiate(explosionEffectPrefab, centerPos, Quaternion.identity, transform);
               }
           })
           .AppendInterval(0.2f) // 爆発を少し見てからテキスト表示
           .Append(itemNameText.DOFade(1f, duration))
           .Join(messageText.DOFade(1f, duration))
           .AppendInterval(1.5f) // 1.5秒待機
           .Append(GetComponent<CanvasGroup>().DOFade(0, duration)) // 全体をフェードアウト
           .OnComplete(() =>
           {
               // 演出が終わったらPivotを元に戻しておくことが重要
               iconRect.pivot = new Vector2(0.5f, 0.5f);
               gameObject.SetActive(false);
           });
    }

    public void Panel_Open(Transform panelTransform)
    {
        panelTransform.gameObject.SetActive(true);
        CanvasGroup canvasGroup = panelTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            panelTransform.gameObject.AddComponent<CanvasGroup>();
        }

        // 初期状態を設定
        canvasGroup.alpha = 0f;
        panelTransform.localScale = Vector3.one * 0.9f;

        // アニメーション
        canvasGroup.DOFade(1f, 0.6f).SetEase(Ease.Linear);
        panelTransform.DOScale(1f, 0.6f).SetEase(Ease.OutQuint);
    }

    public void Panel_Close(Transform panelTransform)
    {
        CanvasGroup canvasGroup = panelTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            panelTransform.gameObject.AddComponent<CanvasGroup>();
        }
        // アニメーション
        canvasGroup.DOFade(0f, 0.5f);
        Vector3 originalPosition = panelTransform.localPosition;
        Vector3 originalScale = panelTransform.localScale;
        panelTransform.DOScale(0.8f, 0.5f); // 少し小さくする
        panelTransform.DOLocalMoveY(panelTransform.localPosition.y + 100f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                panelTransform.gameObject.SetActive(false);
                panelTransform.localPosition = originalPosition; // 元の位置に戻す
                panelTransform.localScale = originalScale; // 元のスケールに戻す
                canvasGroup.alpha = 1f; // アルファ値をリセット
            });
    }

    public void paperOpen(Transform panelTransform)
    {
        panelTransform.gameObject.SetActive(true);

        CanvasGroup canvasGroup = panelTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            panelTransform.gameObject.AddComponent<CanvasGroup>();
        }

        // 初期状態を設定
        canvasGroup.alpha = 0f;
        panelTransform.localScale = Vector3.one;
        Vector3 originalPosition = panelTransform.localPosition;
        // 画面上部から降ってくるようにY座標を大きく上にずらす
        panelTransform.localPosition = new Vector3(originalPosition.x, originalPosition.y + 1200f, originalPosition.z);

        // アニメーション
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear));
        seq.Join(panelTransform.DOLocalMoveY(originalPosition.y, 0.7f).SetEase(Ease.OutSine));
        seq.OnComplete(() =>
        {
            // 最終的に位置を正確に戻す
            panelTransform.localPosition = originalPosition;
        });
        Debug.Log("Paper Open Animation Started");
    }
    public void paperClose(Transform panelTransform)
    {
        CanvasGroup canvasGroup = panelTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            panelTransform.gameObject.AddComponent<CanvasGroup>();
        }
        // アニメーション
        canvasGroup.DOFade(0f, 0.5f);
        Vector3 originalPosition = panelTransform.localPosition;
        Vector3 originalScale = panelTransform.localScale;
        panelTransform.DOScale(0.8f, 0.5f); // 少し小さくする
        panelTransform.DOLocalMoveY(panelTransform.localPosition.y + 100f, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                panelTransform.gameObject.SetActive(false);
                panelTransform.localPosition = originalPosition; // 元の位置に戻す
                panelTransform.localScale = originalScale; // 元のスケールに戻す
                canvasGroup.alpha = 1f; // アルファ値をリセット
            });
    }

    public void Show_notification(string message)
    {
        float FadeInDuration = 0.5f;
        float DisplayDuration = 2.0f;
        float FadeOutDuration = 0.3f;


        if (notificationPrefab == null)
        {
            Debug.LogError("Prefabが読み込まれていないため、通知を生成できません。Initialize処理を確認してください。");
            return;
        }


        // --- ここからが生成と制御の全プロセスです ---

        // 1. Prefabをインスタンス化（シーン内に新しい通知オブジェクトを生成）
        GameObject instance = Instantiate(notificationPrefab);

        // 2. インスタンスから必要なコンポーネントを検索して取得
        CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
        // アニメーションさせるパネルは最初の子要素と仮定
        RectTransform panelTransform = instance.transform.GetChild(0).GetComponent<RectTransform>();
        // テキストは子要素から検索
        TextMeshProUGUI messageText = instance.GetComponentInChildren<TextMeshProUGUI>();

        // 3. 必要なコンポーネントが揃っているか検証
        if (canvasGroup == null || panelTransform == null || messageText == null)
        {
            Debug.LogError("'NotificationPopup' Prefabの構成が正しくありません。PrefabのルートにCanvasGroup、その子にPanel、さらにその子にTextMeshProUGUIが必要です。");
            GameObject.Destroy(instance); // 不完全なインスタンスは破棄
            return;
        }

        // 4. メッセージを設定し、アニメーションの初期状態をセット
        messageText.text = message;
        canvasGroup.alpha = 0f;
        panelTransform.localScale = Vector3.one * 0.7f;

        // 5. DOTweenでアニメーションシーケンスを作成・実行
        Sequence notificationSequence = DOTween.Sequence();
        // シーン遷移時などにTweenがエラーを起こさないよう、ターゲットを設定
        notificationSequence.SetTarget(instance);

        // アニメーション内容 (フェードイン -> 待機 -> フェードアウト)
        notificationSequence.Append(canvasGroup.DOFade(1f, FadeInDuration));
        notificationSequence.Join(panelTransform.DOScale(1f, FadeInDuration).SetEase(Ease.OutBack));
        notificationSequence.AppendInterval(DisplayDuration);
        notificationSequence.Append(canvasGroup.DOFade(0f, FadeOutDuration).SetEase(Ease.InQuad));

        // 6. アニメーション完了時にGameObject自身を破棄
        notificationSequence.OnComplete(() =>
        {
            // Destroy()は即時実行ではないため、nullチェックが有効な場合があります
            if (instance != null)
            {
                GameObject.Destroy(instance);
            }
        });
    }

    #endregion

    #region ロード用

    [Header("アニメーション用スプライト")]
    [SerializeField] private Sprite rocketSprite;
    [SerializeField] Sprite footprintSprite;


    /// <summary>
    /// ロード開始アニメーションを再生します
    /// </summary>
    /// <param name="onComplete">アニメーション完了時に呼び出される処理</param>

    private bool isAnimating = false;
    public void nullLoad_Show()
    {
        Load_Show();
    }

    public void nullLoad_Hide()
    {
        Hide();
    }
    // ロケット画像の角度とアニメーションを正しく制御する
    // ロード開始アニメーションを再生します
    public void Load_Show(Action onComplete = null)
    {
        if (isAnimating) return;
        isAnimating = true;

        // "LoadAnimationRoot"が存在するか確認

        GameObject loadRoot = new GameObject("LoadAnimationRoot");
        DontDestroyOnLoad(loadRoot);
        loadRoot.AddComponent<LoadAnimation>();

        LoadAnimation loadAnim = loadRoot.GetComponent<LoadAnimation>();
        if (loadAnim != null)
        {
            loadAnim.rocketSprite = rocketSprite;
            loadAnim.footprintSprite = footprintSprite;
            loadAnim.Loadshow(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
            });
        }
        else
        {
            isAnimating = false;
            onComplete?.Invoke();
        }
    }

    public void Hide(Action onComplete = null)
    {
        // ロードオブジェクトを探す
        GameObject loadRoot = GameObject.Find("LoadAnimationRoot");
        if (loadRoot == null)
        {
            // なかった場合はリターン
            isAnimating = false;
            onComplete?.Invoke();
            Debug.Log("LoadAnimationRootが見つかりません。アニメーションを終了できません。");
            return;
        }
        LoadAnimation loadAnim = loadRoot.GetComponent<LoadAnimation>();
        if (loadAnim != null)
        {
            loadAnim.Hide(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
            });
        }
        else
        {
            isAnimating = false;
            onComplete?.Invoke();
        }
    }
    #endregion

    #region ガーデン用のボタン


    /// <summary>
    /// ボタンを展開するアニメーション
    /// </summary>
    [SerializeField]
    GameObject buttonUp, buttonLeft, buttonExpand, buttonShrink;
      [Header("アニメーション設定")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Ease expandEase = Ease.OutBack; // 展開時の動き
    [SerializeField] private Ease shrinkEase = Ease.InBack;  // 収納時の動き

    private RectTransform rectOriginal,  rectUp, rectLeft;
    private Vector2 buttonOriginalPosition;
    public void ExpandButtons()
    {   
        rectUp = buttonUp.GetComponent<RectTransform>();
        rectLeft = buttonLeft.GetComponent<RectTransform>();
        rectOriginal = buttonExpand.GetComponent<RectTransform>();  

        // ボタンAの初期位置を保存
        buttonOriginalPosition = rectOriginal.anchoredPosition;
        // 1. ボタン1,2を有効化

        buttonUp.gameObject.SetActive(true);
        buttonLeft.gameObject.SetActive(true);

        // 2. ボタン1,2をボタンAの位置に配置
        rectUp.anchoredPosition = buttonOriginalPosition;
        rectLeft.anchoredPosition = buttonOriginalPosition;

        // 3. ボタン1,2の移動先を動的に計算
        // ボタン1 (上方向) の目標Y座標
        float target_Y = buttonOriginalPosition.y + (rectOriginal.rect.height / 2f) + (rectUp.rect.height / 2f);
        Vector2 targetPosUp = new Vector2(buttonOriginalPosition.x, target_Y);

        // ボタン2 (横方向) の目標X座標
        float target_X = buttonOriginalPosition.x - (rectUp.rect.width / 2f) - (rectLeft.rect.width / 2f);
        Vector2 targetPosLeft = new Vector2(target_X, buttonOriginalPosition.y);

        // DOTweenでアニメーション開始
        rectUp.DOAnchorPos(targetPosUp, animationDuration).SetEase(expandEase);
        rectLeft.DOAnchorPos(targetPosLeft, animationDuration).SetEase(expandEase);

        // 4. ボタンBを有効化
        buttonShrink.gameObject.SetActive(true);
    }

    /// <summary>
    /// ボタンを収納するアニメーション
    /// </summary>
    public void ShrinkButtons()
    {
        buttonShrink.gameObject.SetActive(false);
        
        // 2つのボタンをボタンAの初期位置に戻す
        rectUp.DOAnchorPos(buttonOriginalPosition, animationDuration).SetEase(shrinkEase);
        
        // どちらかのアニメーション完了後(OnComplete)に、オブジェクトの状態を整理する
        rectLeft.DOAnchorPos(buttonOriginalPosition, animationDuration).SetEase(shrinkEase)
            .OnComplete(() =>
            {
                // アニメーション完了後に初期状態に戻す
                buttonUp.gameObject.SetActive(false);
                buttonLeft.gameObject.SetActive(false);
                buttonExpand.gameObject.SetActive(true); // ボタンAを再度有効化
                buttonShrink.gameObject.SetActive(false); // ボタンBを無効化
            });
    }
    #endregion
}