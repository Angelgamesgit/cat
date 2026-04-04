using UnityEngine;

public class SphereCanvasButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    SphereSpec sphereSpec;
    [SerializeField]
      Michsky.UI.Shift.ChapterButton chapterButton;
public void SetStart(SphereSpec sphereSpec)
{
    //スフィアのデータを受け取り、ボタンの画像を変更する
    this.sphereSpec = sphereSpec;
    chapterButton.backgroundImage = sphereSpec.icon;
    chapterButton.buttonTitle = sphereSpec.sphereName;
}


public void SphereChange()
{
    // スフィアの変更
    UISystem uiSystem = FindFirstObjectByType<UISystem>();
    StartCoroutine(uiSystem.SphereChangeAnimation(sphereSpec));
    // スフィア変更パネルを閉じる
    //スフィアを変更する
}
}
