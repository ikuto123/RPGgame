using UnityEngine;

public abstract class UIViewBase : MonoBehaviour
{
    public virtual void Initialize() { } 
    
    //表示処理
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    
    //非表示処理
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    
    //表示されているか確認
    public bool IsVisible => gameObject.activeSelf;
}
