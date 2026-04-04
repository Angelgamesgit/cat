using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Events;

public class UIPageController : MonoBehaviour
{
    public RectTransform[] pages;
    public float slideDuration = 0.5f;
    float slideDistance; // 画面幅（変更可）

    private int currentIndex = 0;
    [SerializeField]
    Sprite QuestionMark;
    [SerializeField]
    GameSystem gameSystem;
     [SerializeField]
    Michsky.UI.Shift.HorizontalSelector selector;

PlayerData playerData;
    void Start()
    {
        slideDistance = Screen.width;
        for (int i = 0; i < pages.Length; i++)
        {
            CanvasGroup cg = pages[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = pages[i].gameObject.AddComponent<CanvasGroup>();
            cg.alpha = (i == currentIndex) ? 1 : 0;
            pages[i].anchoredPosition = Vector2.zero;
            pages[i].gameObject.SetActive(i == currentIndex);
        }

        selector.enabled = false;
        playerData = gameSystem.playerData;
        for(int i = 0; i< playerData.FindSphereList.Count; i++)
        {
            SphereSpec spec = playerData.FindSphereList[i];
            //selector.AddItem(spec.sphereName, this,i);
            for(int s = 0; s < spec.findCatsDate.Count; s++)
            {
                GameObject icon = Instantiate(new GameObject(), pages[i].transform.GetChild(0));
                Image image = icon.AddComponent<Image>();
                if (playerData.catFound.Contains(spec.findCatsDate[s]))
                {
                   // image.sprite  =  Sprite.Create(spec.findCatsDate[s].capturedTexture, 
                    //new Rect(0, 0, spec.findCatsDate[s].capturedTexture.width, spec.findCatsDate[s].capturedTexture.height), Vector2.zero);
                }
                else
                {
                    image.sprite  = QuestionMark;
                }
            }
        }
        selector.enabled = true;
    }

    public void GoToNextPage()
    {
        int next = currentIndex + 1;
        if (next < pages.Length)
        {
            AnimatePageChange(currentIndex, next, +1);
            currentIndex = next;
        }
    }

    public void GoToPreviousPage()
    {
        int prev = currentIndex - 1;
        if (prev >= 0)
        {
            AnimatePageChange(currentIndex, prev, -1);
            currentIndex = prev;
        }
    }

    private void AnimatePageChange(int from, int to, int direction)
    {
        RectTransform fromPage = pages[from];
        RectTransform toPage = pages[to];

        toPage.gameObject.SetActive(true);
        toPage.anchoredPosition = new Vector2(direction * slideDistance, 0);

        CanvasGroup fromGroup = fromPage.GetComponent<CanvasGroup>();
        CanvasGroup toGroup = toPage.GetComponent<CanvasGroup>();
        toGroup.alpha = 0;

        

        // 並行してアニメーション
        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Join(fromPage.DOAnchorPos(new Vector2(-direction * slideDistance, 0), slideDuration));
        seq.Join(toPage.DOAnchorPos(Vector2.zero, slideDuration));
        seq.Join(fromGroup.DOFade(0, slideDuration));
        seq.Join(toGroup.DOFade(1, slideDuration));
        seq.OnComplete(() =>
        {
            fromPage.gameObject.SetActive(false);
        });
    }
    public void OnPageClick(int index)
    {
        if (index < 0 || index >= pages.Length) return;
         AnimatePageChange(currentIndex, index, index > currentIndex ? 1 : -1);
        // クリック時の処理をここに追加
        currentIndex = index;
        Debug.Log("Page clicked: " + currentIndex);
    }
}
