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
    PlayerEquipment = 4, // プレイヤー装備（売却モードでは非表示）
    Setting = 5 // 設定（売却モードでは非表示）
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

    private int GetSouldItemTab = 4;

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

        _amountSelectController = new ItemAmountSelectController(_confirmationPopup, _sellText, popUpItems, _sellCountGroup);

        _initialContentPos = _contentRect.anchoredPosition;
        _confirmationPopup.transform.gameObject.SetActive(false);
        
    }

    // 親から呼ばれる：アイテム一覧を構築して表示だけ行う
    public void ShowPreview(ItemMenuCategory category, PlayerItemMenuMode mode)
    {
        Show();
        _currentMode = mode;
        
        if (category == ItemMenuCategory.Item) SetItemData();
        else SetEquipmentData(category);

        ResetScroll();
        
        // 切り替えた直後は一番上のアイテム情報を表示する
        _gridIndex = 0;
        OnSelectionChanged(_gridIndex);
    }

    // 親から呼ばれる：アイテムの選択処理を開始する
    // 戻り値(bool)は、タブ選択に戻るかどうか（true=戻る）
    public async UniTask<bool> StartSelectionAsync(ItemMenuCategory category, PlayerItemMenuMode mode, ShopInventory shopInventory, CancellationToken token)
    {
        _currentMode = mode;

        // 【機能】空の場合は即座にタブ選択へ戻す
        if (_playerItems == null || _playerItems.Count == 0) return true;

        _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
        Scroll(_gridIndex);

        while (!token.IsCancellationRequested)
        {
            int r = await _selectInput.WaitForSelection(
                _playerItems, token,
                onSelectionChanged: OnSelectionChanged,
                startIndex: _gridIndex, layout: SelectLayout.Grid, columns: 5, canEscapeDown: false, canEscapeUp: true
            );

            _selectInput.ClearSelection();

            // 【機能】Bボタン/ESC(-1)、または上キー(-3)でタブ選択に戻る
            if (r == -1 || r == -3)
            {
                return true;
            }

            _gridIndex = r;

            if (_currentMode == PlayerItemMenuMode.Sell)
            {
                int sellPrice = 0;
                int maxAmount = 1;

                if (category == ItemMenuCategory.Item)
                {
                    ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                    sellPrice = selectedItem.Price / 2;
                    maxAmount = _playerItemInventory[_gridIndex].quantity; 
                }
                else
                {
                    EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                    sellPrice = selectedEquip.Price / 2;
                    maxAmount = 1; 
                }

                int sellAmount = await _amountSelectController.WaitForConfirmationAsync(maxAmount, token);

                if (sellAmount <= 0) continue;

                // 売却処理
                if (category == ItemMenuCategory.Item)
                {
                    ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                    playerItemInventory.RemoveItem(selectedItem, sellAmount); 
                    if (sellPrice > 0) playerStatus.UpCoin(sellPrice * sellAmount); 
                }
                else
                {
                    EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                    playerEquipmentInventory.RemoveEquipment(selectedEquip);
                    if (sellPrice > 0) playerStatus.UpCoin(sellPrice);
                }

                PlayerCoinText.text = playerStatus.Coin.ToString();
                
                // 売却後にリストを再構築
                if (category == ItemMenuCategory.Item) SetItemData();
                else SetEquipmentData(category);
                
                // 【機能】売却後にアイテムが空になったらタブ選択へ戻す
                if (_playerItems == null || _playerItems.Count == 0)
                {
                    return true;
                }
                else
                {
                    _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
                    Scroll(_gridIndex);
                    OnSelectionChanged(_gridIndex);
                }
            }
        }

        return true;
    }

    private void OnSelectionChanged(int index)
    {
        if (_playerItems == null || _playerItems.Count <= index)
        {
            // アイテムがない場合は説明文や画像を空にする（初期化時のエラー防止）
            ItemImage.sprite = null;
            _productDescriptionText.text = "";
            return;
        }

        // 判定に _currentMode ではなく実際に今表示しているリストの要素を使う
        // 現在は SetItemData または SetEquipmentData でリストを上書きしているのでそれを利用
        if (_playerItemInventory != null && _playerItemInventory.Count > index && _playerItembox.transform.GetChild(index).gameObject.activeSelf && _playerItembox.transform.GetChild(index).GetChild(1).GetComponent<TextMeshProUGUI>().text != "1") // 簡易的なアイテム/装備の判定（装備は常に個数1と表示しているため）
        {
             ItemData itemData = _playerItemInventory[index].itemData;
             ItemImage.sprite = itemImageData.GetSprite(itemData.Id);
             _productDescriptionText.text = itemData.Explanation;
        }
        else if (_playerEquipmentInventory != null && _playerEquipmentInventory.Count > index)
        {
             ItemImage.sprite = itemImageData.GetSprite(_playerEquipmentInventory[index].Id);
             _productDescriptionText.text = _playerEquipmentInventory[index].Explanation.ToString();
        }

        Scroll(index);
    }

    private void ResetBox()
    {
        foreach (Transform child in _playerItembox.transform) child.gameObject.SetActive(false);
    }

    private void SetItemData()
    {
        ResetBox();
        PlayerCoinText.text = playerStatus.Coin.ToString();
        _playerItemInventory = playerItemInventory.GetItemInventory();
        _playerEquipmentInventory = new List<EquipmentData>();
        
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
        _playerItemInventory = new List<InventorySlot>();
        
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