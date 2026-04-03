using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerMenuView : UIViewBase
{
    [SerializeField] private GameObject _menuTab;

    private List<UISelectable> _menuTabs = new List<UISelectable>();
    private SelectInput _selectInput = new SelectInput();
    private CancellationTokenSource _cts;

    private int _tabIndex = 0;
    private int _getSouldItemTab = 4;
    
    public override void Show()
    {
        base.Show();
        UIManager.Instance.PlayerStop = true;
        foreach (Transform child in _menuTab.transform) child.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        base.Hide();
        UIManager.Instance.PlayerStop = false;
    }

    public async UniTask StartSelectPlayerMenu(PlayerItemMenuMode mode, ShopInventory shopInventory = null)
    {
        Show();
        _cts = new CancellationTokenSource();

        // 売却モード時は「プレイヤー装備」「設定」などを非表示にする
        if (mode == PlayerItemMenuMode.Sell)
        {
            for (int i = _getSouldItemTab; i < _menuTab.transform.childCount; i++)
                _menuTab.transform.GetChild(i).gameObject.SetActive(false);
        }

        _menuTabs = _menuTab.GetComponentsInChildren<UISelectable>(false).ToList();
        _tabIndex = Mathf.Clamp(_tabIndex, 0, _menuTabs.Count - 1);

        // 初期のタブ内容を表示
        UpdateTabPreview(_tabIndex, mode);

        bool selectingTabs = true;

        while (!_cts.IsCancellationRequested)
        {
            await UniTask.Yield();

            if (selectingTabs)
            {
                // 【タブ選択ステート】
                int r = await _selectInput.WaitForSelection(
                    _menuTabs, _cts.Token,
                    onSelectionChanged: (index) => { 
                        if (_tabIndex != index) { 
                            _tabIndex = index; 
                            UpdateTabPreview(_tabIndex, mode); // タブ移動に合わせて裏で中身を切り替える
                        } 
                    },
                    startIndex: _tabIndex, layout: SelectLayout.Horizontal, columns: 1, canEscapeDown: true 
                );

                _selectInput.ClearSelection();

                // Bボタン/ESC でメニュー全体を閉じる
                if (r == -1) break; 
                
                // 下キー(-2) または 決定キー(0以上) でアイテム選択へ移行
                if (r == -2 || r >= 0)
                {
                    if (r >= 0) _tabIndex = r;
                    selectingTabs = false;
                }
            }
            else
            {
                // 【アイテム選択（子View）ステート】
                ItemMenuCategory category = (ItemMenuCategory)_tabIndex;

                if (category >= ItemMenuCategory.Item && category <= ItemMenuCategory.Accessory)
                {
                    // PlayerItemView に処理を委譲し、終わるまで待つ。
                    // 戻り値(true)は「キャンセル/上キーでタブ選択に戻った」ことを意味する
                    bool returnToTab = await UIManager.Instance.PlayerItemView.StartSelectionAsync(category, mode, shopInventory, _cts.Token);
                    
                    if (returnToTab) selectingTabs = true;
                }
                else if (category == ItemMenuCategory.Setting)
                {
                    bool returnToTab = await UIManager.Instance.SettingView.StartSelectSetting(_cts.Token);
                    if (returnToTab) selectingTabs = true;
                    
                }
                else if(category == ItemMenuCategory.PlayerEquipment)
                {
                    bool returnToTab = await UIManager.Instance.PlayerEquipmentView.StartSelectionAsync(mode, _cts.Token);
                    if (returnToTab) selectingTabs = true;
                }
                else
                {
                    selectingTabs = true;
                }
            }
        }

        // ループを抜けた時（B/ESCで閉じた時）にUIを隠す
        Hide();
        UIManager.Instance.PlayerItemView.Hide(); // 子Viewも閉じる

        if (mode == PlayerItemMenuMode.Sell)
        {
            UIManager.Instance.SelectShopActionView.StartSelectAction(shopInventory).Forget();
        }
    }

    // タブを移動した際に、対応する子Viewに表示を切り替えさせる
    private void UpdateTabPreview(int tabIndex, PlayerItemMenuMode mode)
    {
        ItemMenuCategory category = (ItemMenuCategory)tabIndex;

        if (category >= ItemMenuCategory.Item && category <= ItemMenuCategory.Accessory)
        {
            UIManager.Instance.SettingView.Hide();
            UIManager.Instance.PlayerEquipmentView.Hide();
            UIManager.Instance.PlayerItemView.ShowPreview(category, mode);
        }
        else if (category == ItemMenuCategory.Setting)
        {
            UIManager.Instance.PlayerItemView.Hide();
            UIManager.Instance.PlayerEquipmentView.Hide();
            UIManager.Instance.SettingView.ShowPreview();
        }
        else if(category == ItemMenuCategory.PlayerEquipment)
        {
            UIManager.Instance.PlayerItemView.Hide();
            UIManager.Instance.SettingView.Hide();
            UIManager.Instance.PlayerEquipmentView.ShowPreview();
        }
    }

    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
