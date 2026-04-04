using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    // Updateで毎フレーム監視することも可能
    // void Update() { ApplySafeArea(); }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        if (safeArea != lastSafeArea)
        {
            lastSafeArea = safeArea;
            
            // セーフエリアのピクセル座標を、Canvasのアンカー座標(0-1)に変換
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // アンカーをセーフエリアに設定
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            
            // アンカーを設定した後は位置やサイズをリセット
            panel.anchoredPosition = Vector2.zero;
            panel.sizeDelta = Vector2.zero;
        }
    }
}