using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public enum ItemMenuCategory
{
    Item = 0, // アイテム
    Weapon = 1, // 武器
    Armor = 2, // 防具
    Accessory = 3, // アクセサリー
    Setting = 4 // 設定（売却モードでは非表示）
}

public enum PlayerItemMenuMode
{
    View, //アイテムの閲覧モード
    Sell //アイテムの売却モード
}

public class PlayerItemView : UIViewBase
{
    [Header("UI References")]
    [SerializeField] private GameObject _playerItembox;
    [SerializeField] private Image ItemImage;
    [SerializeField] private TextMeshProUGUI PlayerCoinText;
    [SerializeField] private RectTransform _viewportRect;
    [SerializeField] private RectTransform _contentRect;
    [SerializeField] private GameObject _menuTab;

    [Header("Popup UI")]
    [SerializeField] private GameObject _confirmationPopup;
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private TextMeshProUGUI _sellText;
    [SerializeField] private GameObject _SellCountButton;
    [SerializeField] private GameObject _sellCountGroup; // 売却個数選択UI全体
    [SerializeField] private TextMeshProUGUI _productDescriptionText;

    [Header("Data References")]
    [SerializeField] private ItemDataUnit itemDataUnit;
    [SerializeField] private EquipmentDataUnit equipmentDataUnit;
    [SerializeField] private ItemImageData itemImageData;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
    [SerializeField] private PlayerItemInventory playerItemInventory;

    private float topPadding = 20f; 
    private float bottomPadding = 20f;
    private Vector2 _initialContentPos;

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();
    
    // 分離したポップアップ専用のロジッククラス
    private ItemAmountSelectController _amountSelectController;

    private List<UISelectable> _playerItems = new List<UISelectable>();
    private List<InventorySlot> _playerItemInventory = new List<InventorySlot>();
    private List<EquipmentData> _playerEquipmentInventory = new List<EquipmentData>();
    private List<UISelectable> _menuTabs = new List<UISelectable>();

    private int _tabIndex = 0;
    private int _gridIndex = 0;
    private PlayerItemMenuMode _currentMode;

    void Awake()
    {
        var confirmButtons = _confirmButton.GetComponentsInChildren<UISelectable>(true).ToList();
        var sellCountButtons = _SellCountButton.GetComponentsInChildren<UISelectable>(true).ToList();

        List<UISelectable> popUpItems = new List<UISelectable>();
        popUpItems.AddRange(sellCountButtons);
        popUpItems.AddRange(confirmButtons);

        // 純粋なC#クラス（ロジック）を生成して参照を渡す
        _amountSelectController = new ItemAmountSelectController(_confirmationPopup, _sellText, popUpItems, _sellCountGroup);

        _initialContentPos = _contentRect.anchoredPosition;
        _confirmationPopup.transform.gameObject.SetActive(false);
        Debug.Log("初期化完了");
        Hide();
    }

    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    private ItemMenuCategory TabIndexToCategory(int tabIndex) => (ItemMenuCategory)tabIndex;

    private void BuildContentsByTab(int tabIndex)
    {
        var cat = TabIndexToCategory(tabIndex);

        if (cat == ItemMenuCategory.Item) SetItemData();
        else SetEquipmentData(cat);

        ResetScroll();
    }

    public override void Show()
    {
        base.Show();

        foreach (Transform child in _menuTab.transform) child.gameObject.SetActive(true);

        if (_currentMode == PlayerItemMenuMode.Sell)
            _menuTab.transform.GetChild(_menuTab.transform.childCount - 1).gameObject.SetActive(false);

        _menuTabs = _menuTab.GetComponentsInChildren<UISelectable>(false).ToList();
    }

    private void ResetBox()
    {
        foreach (Transform child in _playerItembox.transform) child.gameObject.SetActive(false);
    }

    private void OnSelectionChanged(int index)
    {
        _gridIndex = index;

        if (_playerItems == null || _playerItems.Count <= index) return;
        var category = TabIndexToCategory(_tabIndex);

        if (category == ItemMenuCategory.Item)
        {
            if (_playerItemInventory.Count > index)
            {
                ItemData itemData = _playerItemInventory[index].itemData;
                ItemImage.sprite = itemImageData.GetSprite(itemData.Id);
                _productDescriptionText.text = itemData.Explanation;
            }
        }
        else
        {
            if (_playerEquipmentInventory.Count > index)
            {
                ItemImage.sprite = itemImageData.GetSprite(_playerEquipmentInventory[index].Id);
                _productDescriptionText.text = _playerEquipmentInventory[index].Explanation.ToString();
            }
        }

        Scroll(index);
    }

    private void SetItemData()
    {
        ResetBox();
        PlayerCoinText.text = playerStatus.Coin.ToString();
        _playerItemInventory = playerItemInventory.GetItemInventory();

        for (int i = 0; i < _playerItemInventory.Count; i++)
        {
            _playerItembox.transform.GetChild(i).gameObject.SetActive(true);
            _playerItembox.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().sprite = itemImageData.GetSprite(_playerItemInventory[i].itemData.Id);
            _playerItembox.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = _playerItemInventory[i].quantity.ToString();
            _playerItembox.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = (_playerItemInventory[i].itemData.Price / 2).ToString();
        }

        _playerItems = _playerItembox.GetComponentsInChildren<UISelectable>(true).Where(item => item.gameObject.activeSelf).ToList();
    }

    private void SetEquipmentData(ItemMenuCategory type)
    {
        ResetBox();
        PlayerCoinText.text = playerStatus.Coin.ToString();

        switch (type)
        {
            case ItemMenuCategory.Weapon: _playerEquipmentInventory = playerEquipmentInventory.GetWeaponsInventory(); break;
            case ItemMenuCategory.Armor: _playerEquipmentInventory = playerEquipmentInventory.GetArmorsInventory(); break;
            case ItemMenuCategory.Accessory: _playerEquipmentInventory = playerEquipmentInventory.GetAccessoriesInventory(); break;
            default: return;
        }

        for (int i = 0; i < _playerEquipmentInventory.Count; i++)
        {
            _playerItembox.transform.GetChild(i).gameObject.SetActive(true);
            _playerItembox.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().sprite = itemImageData.GetSprite(_playerEquipmentInventory[i].Id);
            _playerItembox.transform.GetChild(i).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = 1.ToString();
            _playerItembox.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = (_playerEquipmentInventory[i].Price / 2).ToString();
        }

        _playerItems = _playerItembox.GetComponentsInChildren<UISelectable>(true).Where(item => item.gameObject.activeSelf).ToList();
    }

    public async UniTask StartSelectPlayerInventory(PlayerItemMenuMode mode)
    {
        _currentMode = mode;
        Show();

        _tabIndex = Mathf.Clamp(_tabIndex, 0, _menuTabs.Count - 1);
        BuildContentsByTab(_tabIndex);

        bool selectingTabs = true;

        while (!_cts.IsCancellationRequested)
        {
            await UniTask.Yield();

            if (selectingTabs)
            {
                int r = await _selectInput.WaitForSelection(
                    _menuTabs, _cts.Token,
                    onSelectionChanged: (index) => { if (_tabIndex != index) { _tabIndex = index; BuildContentsByTab(_tabIndex); } },
                    startIndex: _tabIndex, layout: SelectLayout.Horizontal, columns: 1, canEscapeDown: true 
                );

                _selectInput.ClearSelection();

                // 【機能】タブ選択中にBボタン/ESCで完全に画面を閉じる
                if (r == -1) break; 
                
                if (r == -2)
                {
                    if (_playerItems == null || _playerItems.Count == 0) continue;
                    _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
                    Scroll(_gridIndex);
                    selectingTabs = false;
                    continue;
                }

                _tabIndex = r;
                if (_playerItems == null || _playerItems.Count == 0) continue;

                selectingTabs = false;
            }
            else
            {
                int r = await _selectInput.WaitForSelection(
                    _playerItems, _cts.Token,
                    onSelectionChanged: OnSelectionChanged,
                    startIndex: _gridIndex, layout: SelectLayout.Grid, columns: 5, canEscapeDown: false, canEscapeUp: true
                );

                _selectInput.ClearSelection();

                // 【機能】アイテム選択中にBボタン/ESC、または上キーでタブ選択に戻る
                if (r == -1 || r == -3)
                {
                    selectingTabs = true;
                    continue;
                }

                _gridIndex = r;

                if (_currentMode == PlayerItemMenuMode.Sell)
                {
                    var category = TabIndexToCategory(_tabIndex);
                    string itemName = "";
                    int sellPrice = 0;
                    int maxAmount = 1;

                    if (category == ItemMenuCategory.Item)
                    {
                        ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                        itemName = selectedItem.Name;
                        sellPrice = selectedItem.Price / 2;
                        maxAmount = _playerItemInventory[_gridIndex].quantity; 
                    }
                    else
                    {
                        EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                        itemName = selectedEquip.Name;
                        sellPrice = selectedEquip.Price / 2;
                        maxAmount = 1; 
                    }

                    // 分離したクラスに処理を任せる！
                    int sellAmount = await _amountSelectController.WaitForConfirmationAsync(maxAmount, _cts.Token);

                    if (sellAmount <= 0) continue;

                    // 【機能】0Gの場合はコインを増やさない安全な売却（処分）処理
                    if (category == ItemMenuCategory.Item )
                    {
                        ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                        playerItemInventory.RemoveItem(selectedItem, sellAmount); 
                        if (sellPrice > 0) playerStatus.UpCoin(sellPrice * sellAmount); 
                        Debug.Log($"{itemName} を {sellAmount}個 売却(処分)しました");
                    }
                    else
                    {
                        EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                        playerEquipmentInventory.RemoveEquipment(selectedEquip);
                        if (sellPrice > 0) playerStatus.UpCoin(sellPrice);
                        Debug.Log($"{itemName} を 売却(処分)しました");
                    }

                    PlayerCoinText.text = playerStatus.Coin.ToString();
                    BuildContentsByTab(_tabIndex);
                    
                    // 【機能】売却後にアイテムが空になったらタブ選択へ戻す
                    if (_playerItems == null || _playerItems.Count == 0)
                    {
                        selectingTabs = true;
                    }
                    else
                    {
                        _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
                        Scroll(_gridIndex);
                    }
                }
            }
        }
        
        // ループを抜けた時（B/ESCで閉じた時）にUIを隠す
        Hide();
        // ※必要であればここに UIManager.Instance.ShopActionView.Show(); 等の前の画面に戻す処理を追加します
    }

    private void ResetScroll()
    {
        if (_contentRect != null) _contentRect.anchoredPosition = _initialContentPos;
    }

    private void Scroll(int index)
    {
        if (_viewportRect == null || _contentRect == null || _playerItems == null || _playerItems.Count <= index) return;
        if (index == 0) { ResetScroll(); return; }

        Canvas.ForceUpdateCanvases();
        RectTransform selectedRect = _playerItems[index].GetComponent<RectTransform>();
        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];

        _viewportRect.GetWorldCorners(viewportCorners);
        selectedRect.GetWorldCorners(itemCorners);

        float itemBottom = itemCorners[0].y;
        float itemTop = itemCorners[1].y;
        float viewportTop = viewportCorners[1].y - topPadding;
        float viewportBottom = viewportCorners[0].y + bottomPadding;

        if (itemTop > viewportTop)
        {
            _contentRect.position -= new Vector3(0, itemTop - viewportTop, 0);
        }
        else if (itemBottom < viewportBottom)
        {
            _contentRect.position += new Vector3(0, viewportBottom - itemBottom, 0);
        }
    }
}