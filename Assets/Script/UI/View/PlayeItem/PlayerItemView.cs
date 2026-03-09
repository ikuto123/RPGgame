using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public enum ItemMenuCategory
{
    Item = 0, // リンゴ（消費アイテムなど）
    Weapon = 1, // 剣（武器）
    Armor = 2, // 鎧（防具）
    Accessory = 3, // 鈴（アクセサリー）
    Material = 4 // 歯車（素材や設定など）
}

public enum PlayerItemMenuMode
{
    View, //アイテムの閲覧モード
    Sell //アイテムの売却モード
}

public class PlayerItemView : UIViewBase
{
    [SerializeField] private GameObject _playerItembox;

    [SerializeField] private Image ItemImage;
    [SerializeField] private TextMeshProUGUI PlayerCoinText;

    [SerializeField] private RectTransform _viewportRect;
    [SerializeField] private RectTransform _contentRect;

    private float topPadding = 20f; // 余白（デザインに合わせて調整してください）
    private float bottomPadding = 20f;
    private Vector2 _initialContentPos;

    [SerializeField] private ItemDataUnit itemDataUnit;
    [SerializeField] private EquipmentDataUnit equipmentDataUnit;

    [SerializeField] private ItemImageData itemImageData;

    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
    [SerializeField] private PlayerItemInventory playerItemInventory;

    [SerializeField] private GameObject _menuTab;

    [SerializeField] private GameObject _confirmationPopup;
    [SerializeField] private GameObject _confirmButton;

    [SerializeField] private TextMeshProUGUI _sellText;
    [SerializeField] private GameObject _SellCountButton;

    [SerializeField] private TextMeshProUGUI _productDescriptionText;
    
    [SerializeField] private GameObject _sellCountGroup; // 売却個数選択UI全体をまとめるオブジェクト


    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();

    private List<UISelectable> _playerItems = new List<UISelectable>();

    private List<InventorySlot> _playerItemInventory = new List<InventorySlot>();

    private List<EquipmentData> _playerEquipmentInventory = new List<EquipmentData>();

    private List<UISelectable> _menuTabs = new List<UISelectable>();

    private List<UISelectable> _confirmButtons = new List<UISelectable>();

    private List<UISelectable> _SellCountButtons = new List<UISelectable>();

    private int _tabIndex = 0;
    private int _gridIndex = 0;

    private PlayerItemMenuMode _currentMode;

    void Awake()
    {
        _confirmButtons = _confirmButton.GetComponentsInChildren<UISelectable>(true).ToList();
        _SellCountButtons = _SellCountButton.GetComponentsInChildren<UISelectable>(true).ToList();

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

        if (cat == ItemMenuCategory.Item || cat == ItemMenuCategory.Material)
        {
            SetItemData();
        }
        else
        {
            SetEquipmentData(cat);
        }

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

        if (category == ItemMenuCategory.Item || category == ItemMenuCategory.Material)
        {
            // 消費アイテム・素材の場合
            if (_playerItemInventory.Count > index)
            {
                ItemData itemData = _playerItemInventory[index].itemData;
                ItemImage.sprite = itemImageData.GetSprite(itemData.Id);
                _productDescriptionText.text = itemData.Explanation;
            }
        }
        else
        {
            // 武器・防具・アクセサリーの場合
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

            _playerItembox.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().sprite
                = itemImageData.GetSprite(_playerItemInventory[i].itemData.Id);

            _playerItembox.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text
                = _playerItemInventory[i].quantity.ToString();

            _playerItembox.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text
                = (_playerItemInventory[i].itemData.Price / 2).ToString();
        }

        _playerItems = _playerItembox.GetComponentsInChildren<UISelectable>(true)
            .Where(item => item.gameObject.activeSelf)
            .ToList();
    }

    private void SetEquipmentData(ItemMenuCategory type)
    {
        ResetBox();
        PlayerCoinText.text = playerStatus.Coin.ToString();

        switch (type)
        {
            case ItemMenuCategory.Weapon: //武器
                _playerEquipmentInventory = playerEquipmentInventory.GetWeaponsInventory();
                break;
            case ItemMenuCategory.Armor: //防具
                _playerEquipmentInventory = playerEquipmentInventory.GetArmorsInventory();
                break;
            case ItemMenuCategory.Accessory: //アクセサリー
                _playerEquipmentInventory = playerEquipmentInventory.GetAccessoriesInventory();
                break;
            default:
                return;
        }

        for (int i = 0; i < _playerEquipmentInventory.Count; i++)
        {
            _playerItembox.transform.GetChild(i).gameObject.SetActive(true);

            _playerItembox.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().sprite
                = itemImageData.GetSprite(_playerEquipmentInventory[i].Id);

            _playerItembox.transform.GetChild(i).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text
                = 1.ToString();

            _playerItembox.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text
                = (_playerEquipmentInventory[i].Price / 2).ToString();
        }

        _playerItems = _playerItembox.GetComponentsInChildren<UISelectable>(true)
            .Where(item => item.gameObject.activeSelf)
            .ToList();
    }

    public async UniTask StartSelectPlayerInventory(PlayerItemMenuMode mode)
    {
        _currentMode = mode;
        Show();

        //最初に「いま選択中のタブ」に合わせて中身を構築
        _tabIndex = Mathf.Clamp(_tabIndex, 0, _menuTabs.Count - 1);
        BuildContentsByTab(_tabIndex);

        bool selectingTabs = true;

        while (!_cts.IsCancellationRequested)
        {
            await UniTask.Yield();

            if (selectingTabs)
            {
                // タブ（横並び）
                int r = await _selectInput.WaitForSelection(
                    _menuTabs,
                    _cts.Token,
                    onSelectionChanged: (index) =>
                    {
                        if (_tabIndex != index)
                        {
                            _tabIndex = index;
                            BuildContentsByTab(_tabIndex);
                        }
                    },
                    startIndex: _tabIndex,
                    layout: SelectLayout.Horizontal,
                    columns: 1,
                    canEscapeDown: true // 下で中身へ
                );

                _selectInput.ClearSelection();

                if (r == -1) break; // B/ESCで閉じる等
                if (r == -2)
                {
                    if (_playerItems == null || _playerItems.Count == 0) continue;
                    _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
                    Scroll(_gridIndex);
                    // 下入力で中身へ移動
                    selectingTabs = false;
                    continue;
                }

                // ここに来るのは決定ボタンを押した時のみ
                _tabIndex = r;

                if (_playerItems == null || _playerItems.Count == 0) continue;


                selectingTabs = false;
            }
            else
            {
                int r = await _selectInput.WaitForSelection(
                    _playerItems,
                    _cts.Token,
                    onSelectionChanged: OnSelectionChanged,
                    startIndex: _gridIndex,
                    layout: SelectLayout.Grid,
                    columns: 5,
                    canEscapeDown: false,
                    canEscapeUp: true
                );

                _selectInput.ClearSelection();

                if (r == -1) break;
                if (r == -3)
                {
                    // 上入力でタブへ戻る
                    selectingTabs = true;
                    continue;
                }

                // 決定されたグリッド index
                _gridIndex = r;

                // TODO: ここにアイテム使用や売却(modeに応じた処理)を実装
                if (_currentMode == PlayerItemMenuMode.Sell)
                {
                    var category = TabIndexToCategory(_tabIndex);

                    string itemName = "";
                    int sellPrice = 0;
                    int maxAmount = 1;

                    // 1. アイテム名、売値、所持している最大個数を取得
                    if (category == ItemMenuCategory.Item || category == ItemMenuCategory.Material)
                    {
                        ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                        itemName = selectedItem.Name;
                        sellPrice = selectedItem.Price / 2;
                        maxAmount = _playerItemInventory[_gridIndex].quantity; // 所持数を取得
                    }
                    else
                    {
                        EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                        itemName = selectedEquip.Name;
                        sellPrice = selectedEquip.Price / 2;
                        maxAmount = 1; // 装備品は基本的に1つ
                    }

                    // 2. ダイアログを待機し、売る【個数】を受け取る（0ならキャンセル）
                    int sellAmount = await Confirmation(itemName, sellPrice, maxAmount);

                    if (sellAmount <= 0)
                    {
                        continue; // 「やめる」が選ばれたら戻る
                    }

                    // 3. 受け取った個数ぶん売却する
                    if (category == ItemMenuCategory.Item || category == ItemMenuCategory.Material)
                    {
                        ItemData selectedItem = _playerItemInventory[_gridIndex].itemData;
                        playerItemInventory.RemoveItem(selectedItem, sellAmount); // 選んだ個数消す
                        playerStatus.UpCoin(sellPrice * sellAmount); // 値段 × 個数ぶん増やす
                        Debug.Log($"{itemName} を {sellAmount}個 売却しました");
                    }
                    else
                    {
                        EquipmentData selectedEquip = _playerEquipmentInventory[_gridIndex];
                        playerEquipmentInventory.RemoveEquipment(selectedEquip);
                        playerStatus.UpCoin(sellPrice);
                        Debug.Log($"{itemName} を 売却しました");
                    }

                    PlayerCoinText.text = playerStatus.Coin.ToString();
                    BuildContentsByTab(_tabIndex);
                    
                    if (_playerItems == null || _playerItems.Count == 0)
                    {
                        selectingTabs = true;
                    }
                    else
                    {
                        // アイテムが減ってカーソルが範囲外（何もない空間）を指さないように調整
                        _gridIndex = Mathf.Clamp(_gridIndex, 0, _playerItems.Count - 1);
                        Scroll(_gridIndex);
                    }
                }
            }
        }
    }

    private async UniTask<int> Confirmation(string itemName, int price, int maxAmount)
    {
        _confirmationPopup.SetActive(true);
        int currentAmount = 1;

        // [0]白枠, [1]上矢印, [2]下矢印, [3]売る, [4]やめる の順番になるようにします
        List<UISelectable> popUpItems = new List<UISelectable>();
        popUpItems.AddRange(_SellCountButtons);
        popUpItems.AddRange(_confirmButtons);

        bool canSelectAmount = maxAmount > 1;
        

        _sellCountGroup.SetActive(canSelectAmount);
        
        
        // 安全対策（要素が足りない場合はエラーを出して戻る）
        if (popUpItems.Count < 5)
        {
            Debug.LogError("ポップアップのUI要素が5つ揃っていません！Inspectorの配置順を確認してください。");
            _confirmationPopup.SetActive(false);
            return 0;
        }

        int currentIndex = canSelectAmount ? 0 : 3;
        float nextInputTime = 0f;

        // 初期選択の見た目をセット
        foreach (var item in popUpItems) item.SetSelected(false);
        popUpItems[currentIndex].SetSelected(true);

        while (!_cts.IsCancellationRequested)
        {
            // 個数テキストの更新
            if (_sellText != null) _sellText.text = currentAmount.ToString();

            await UniTask.Yield();

            // === 決定キーの処理 ===
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                popUpItems[currentIndex].Press(); // 押した演出

                if (currentIndex == 1) // 上矢印で決定を押した
                {
                    currentAmount++;
                    if (currentAmount > maxAmount) currentAmount = maxAmount;
                }
                else if (currentIndex == 2) // 下矢印で決定を押した
                {
                    currentAmount--;
                    if (currentAmount < 1) currentAmount = 1;
                }
                else if (currentIndex == 3) // 売るボタンで決定を押した
                {
                    _confirmationPopup.SetActive(false);
                    return currentAmount; // 選んだ個数を返す！
                }
                else if (currentIndex == 4) // やめるボタンで決定を押した
                {
                    _confirmationPopup.SetActive(false);
                    return 0; // キャンセル扱い
                }
            }

            // === キャンセルキーの処理 ===
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
            {
                _confirmationPopup.SetActive(false);
                return 0;
            }

            // === 十字キーのカスタム移動処理 ===
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if (Time.time >= nextInputTime && (v != 0 || h != 0))
            {
                int nextIndex = currentIndex;

                if (canSelectAmount)
                {
                    if (v > 0.5f) // 【上】を押した時
                    {
                        if (currentIndex == 3) nextIndex = 0; // 売る -> 白枠
                        else if (currentIndex == 4) nextIndex = 2; // やめる -> 下矢印
                        else if (currentIndex == 2) nextIndex = 1; // 下矢印 -> 上矢印
                    }
                    else if (v < -0.5f) // 【下】を押した時
                    {
                        if (currentIndex == 0) nextIndex = 3; // 白枠 -> 売る
                        else if (currentIndex == 1) nextIndex = 2; // 上矢印 -> 下矢印
                        else if (currentIndex == 2) nextIndex = 4; // 下矢印 -> やめる
                    }
                    else if (h > 0.5f) // 【右】を押した時
                    {
                        if (currentIndex == 0) nextIndex = 1; // 白枠 -> 上矢印
                        else if (currentIndex == 3) nextIndex = 4; // 売る -> やめる
                    }
                    else if (h < -0.5f) // 【左】を押した時
                    {
                        if (currentIndex == 1 || currentIndex == 2) nextIndex = 0; // 矢印 -> 白枠
                        else if (currentIndex == 4) nextIndex = 3; // やめる -> 売る
                    }
                }
                else
                {
                    if (h > 0.5f && currentIndex == 3) nextIndex = 4;
                    else if (h < -0.5f && currentIndex == 4) nextIndex = 3;
                }

                // 移動が発生した場合のみカーソルを更新
                if (nextIndex != currentIndex)
                {
                    popUpItems[currentIndex].SetSelected(false);
                    currentIndex = nextIndex;
                    popUpItems[currentIndex].SetSelected(true);
                    nextInputTime = Time.time + 0.2f; // 入力のクールダウン
                }
            }
        }

        return 0;
    }

    private void ResetScroll()
    {
        if (_contentRect != null)
        {
            _contentRect.anchoredPosition = _initialContentPos;
        }
    }

    private void Scroll(int index)
    {
        // 必要な要素がない、またはインデックスが範囲外の場合は何もしない
        if (_viewportRect == null || _contentRect == null || _playerItems == null ||
            _playerItems.Count <= index) return;

        // 最初のアイテムなら強制的に一番上に戻す
        if (index == 0)
        {
            ResetScroll();
            return;
        }

        Canvas.ForceUpdateCanvases();
        RectTransform selectedRect = _playerItems[index].GetComponent<RectTransform>();

        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];

        // ワールド座標を取得（[0]左下、[1]左上、[2]右上、[3]右下）
        _viewportRect.GetWorldCorners(viewportCorners);
        selectedRect.GetWorldCorners(itemCorners);

        float itemBottom = itemCorners[0].y;
        float itemTop = itemCorners[1].y;
        float viewportTop = viewportCorners[1].y - topPadding;
        float viewportBottom = viewportCorners[0].y + bottomPadding;

        if (itemTop > viewportTop)
        {
            float diff = itemTop - viewportTop;
            _contentRect.position -= new Vector3(0, diff, 0);
        }
        else if (itemBottom < viewportBottom)
        {
            float diff = viewportBottom - itemBottom;
            _contentRect.position += new Vector3(0, diff, 0);
        }
    }
}