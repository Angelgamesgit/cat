// === File: Garden_ItemButton.cs ===
// (プロジェクトフォルダ/Scripts/UI/ などに作成してください)
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUI を使用
using System;

public class Garden_ItemButton : MonoBehaviour
{
    [Header("UI References (Inspectorで設定)")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Button buttonComponent;
    [Tooltip("残り個数が0で、配置済みの場合に表示するパネル。")]
    [SerializeField] private GameObject hidePanel; // 通常はImageなど

    private Garden_ItemSO _associatedItem;
    private Action<Garden_ItemSO> _onClickAction;

    void Awake()
    {
        if (buttonComponent == null) buttonComponent = GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogError("Buttonコンポーネントが見つかりません。", this);
        }
        if (hidePanel != null) hidePanel.SetActive(false); // 初期状態は非表示
    }

    public void Setup(Garden_ItemSO itemSO, int remainingCount, bool isPlacedInGarden)
    {
        _associatedItem = itemSO;

        if (itemSO == null)
        {
            if (itemNameText != null) itemNameText.text = "Error";
            if (itemCountText != null) itemCountText.text = "";
            if (itemIconImage != null) itemIconImage.sprite = null;
            if (buttonComponent != null) buttonComponent.interactable = false;
            if (hidePanel != null) hidePanel.SetActive(false);
            Debug.LogWarning("Setupに渡されたGarden_ItemSOがnullです。", this.gameObject);
            return;
        }

        if (itemNameText != null) itemNameText.text = itemSO.displayName;
        if (itemIconImage != null)
        {
            itemIconImage.sprite = itemSO.icon;
            itemIconImage.enabled = (itemSO.icon != null);
        }

        UpdateVisuals(remainingCount, isPlacedInGarden);
    }

    public void SetClickAction(Action<Garden_ItemSO> onClickCallback)
    {
        _onClickAction = onClickCallback;
    }

    public void UpdateVisuals(int remainingCount, bool isPlacedInGarden)
    {
        if (itemCountText != null)
        {
            itemCountText.text = remainingCount.ToString();
        }

        if (buttonComponent != null)
        {
            buttonComponent.interactable = (remainingCount > 0);
        }

        if (hidePanel != null)
        {
            // 残り個数が0で、かつ庭に1つ以上配置されている場合に hidePanel を表示
            hidePanel.SetActive(remainingCount <= 0 && isPlacedInGarden);
        }
    }

    private void HandleClick()
    {
        if (_associatedItem != null && _onClickAction != null)
        {
            // ボタンがインタラクティブでない（残り0など）場合は何もしない
            if (buttonComponent != null && !buttonComponent.interactable) return;

            _onClickAction.Invoke(_associatedItem);
        }
    }
}