using UnityEngine;
using UnityEngine.UI;
using System;

public class CombineDialog : MonoBehaviour
{
    public GameObject root;
    public Button yesButton;
    public Button noButton;

    Action onYes;

    void Awake()
    {
        root.SetActive(false);

        yesButton.onClick.AddListener(() =>
        {
            root.SetActive(false);
            onYes?.Invoke();
        });

        noButton.onClick.AddListener(() =>
        {
            root.SetActive(false);
        });
    }

    public void Show(
        InventoryItemUI a,
        InventoryItemUI b,
        Item result,
        Action yesCallback)
    {
        onYes = yesCallback;
        root.SetActive(true);
    }
}
