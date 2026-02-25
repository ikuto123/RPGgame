using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using TMPro;

public class ItemRewardView : UIViewBase
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panelRoot; //パネル全体
    [SerializeField] private Image iconImage;      //アイテムアイコン

    private Action _onCloseCallback; //閉じた後に実行する処理
    
    public async UniTask ShowReward(string name, Sprite icon, int amount, System.Action onClosed)
    {
        if (iconImage != null) iconImage.sprite = icon;

        _onCloseCallback = onClosed;
        panelRoot.SetActive(true);
        
        // カーソルを表示
        if (UIManager.Instance != null) UIManager.Instance.SetCursorState(true);
        
        await UniTask.WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E));
        
        Close();
    }

    public void Close()
    {
        panelRoot.SetActive(false);
        UIManager.Instance.SetCursorState(false); //カーソルを戻す

        _onCloseCallback?.Invoke();
        _onCloseCallback = null;
    }
}
