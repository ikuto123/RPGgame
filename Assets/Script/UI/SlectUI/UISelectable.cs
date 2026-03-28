using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UISelectable : MonoBehaviour
{
    private Outline _outline;
    private Button _button;

    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private Vector2 outlineDistance = new Vector2(3f, -3f);

    
    private void Awake()
    {
        _button = GetComponent<Button>();
        
        // Outlineコンポーネント取得（なければ追加）
        if (!TryGetComponent(out _outline))
        {
            _outline = gameObject.AddComponent<Outline>();
        }

        _outline.effectColor = outlineColor;
        _outline.effectDistance = outlineDistance;
        _outline.enabled = false; 
    }

    // 外部から呼ばれて選択状態を切り替える
    public void SetSelected(bool isSelected)
    {
        if (_outline != null) _outline.enabled = isSelected;
    }

    // 決定キーが押されたらボタンのクリック処理を実行する
    public void Press()
    {
        _button.onClick.Invoke();
    }
}