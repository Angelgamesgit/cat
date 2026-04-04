
// (プロジェクトフォルダ/Scripts/Controllers/ などに作成してください)
using System.Collections.Generic;
using UnityEngine;

public class Garden_HighlightController : MonoBehaviour
{
    [Tooltip("ハイライト時に使用するマテリアル。")]
    public Material highlightMaterial; // Inspectorで設定

    private List<Renderer> _renderer;
    private List<Material> _originalMaterial;
    private bool _isHighlighted = false;

    void Awake()
    {
        
        _renderer = new List<Renderer>();
        _originalMaterial = new List<Material>();
        foreach (Transform t in transform)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
            {
                _renderer.Add(r);
                _originalMaterial.Add(r.material); // 元のマテリアルを保存
            }
        }
      
    }

    public void Highlight()
    {
        if (_renderer != null && highlightMaterial != null && !_isHighlighted)
        {
            foreach(Renderer r in _renderer)
            {
                    // 元のマテリアルを保持してハイライト用に変更
                    r.material = highlightMaterial;
                
            }
         
            _isHighlighted = true;
        }
    }

    public void Unhighlight()
    {
        if (_renderer != null && _originalMaterial != null && _isHighlighted)
        {
             // Unhighlight() を呼んでも良いが、マテリアルインスタンスの扱いによっては直接戻す
           foreach(Renderer r in _renderer)
            {
                if (r != null && _originalMaterial.Count > 0)
                {
                    r.material = _originalMaterial[_renderer.IndexOf(r)];
                }
            }
            _isHighlighted = false;
        }
    }

    void OnDestroy()
    {
        // オブジェクト破棄時にマテリアルがハイライトされたままになるのを防ぐ
        if (_renderer != null && _originalMaterial != null && _isHighlighted)
        {
             // Unhighlight() を呼んでも良いが、マテリアルインスタンスの扱いによっては直接戻す
           foreach(Renderer r in _renderer)
            {
                if (r != null && _originalMaterial.Count > 0)
                {
                    r.material = _originalMaterial[_renderer.IndexOf(r)];
                }
            }
            _isHighlighted = false;
        }
    }
}