using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentShopView : UIViewBase
{
    [SerializeField] private Image ItemImage;
    [SerializeField] private TextMeshProUGUI PlayerCoinText;
    [SerializeField] private GameObject ProductList;
    
    [SerializeField] private RectTransform _viewportRect; 
    [SerializeField] private RectTransform _contentRect;
    
    private ShopInventory _currentShop; // 統合したデータクラス
    
    [SerializeField] private ItemImageData itemImageData;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerItemInventory playerItemInventory; // PlayerItemInventoryで統一
    [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory; // 装備追加用に追加
    
    [SerializeField] private TextMeshProUGUI _productDescription;
    
    [SerializeField] private TextMeshProUGUI _underText;
    
    [Header("Popup UI")]
    [SerializeField] private GameObject _confirmationPopup;
    [SerializeField] private TextMeshProUGUI _confirmationText;
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private GameObject _SellCountButton; // 追加：個数選択UI
    
    private ItemAmountSelectController _amountSelectController; // 追加：個数選択ロジック
    
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();
    private List<UISelectable> _choiceSelectables = new List<UISelectable>();

    private int _currentIndex = 0;
    private float topPadding = 70f;
    private float bottomPadding = 70f;
    private int _lastIndex = 0; 

    private void Awake()
    {
        var confirmButtons = _confirmButton.GetComponentsInChildren<UISelectable>(true).ToList();
        var sellCountButtons = _SellCountButton.GetComponentsInChildren<UISelectable>(true).ToList();
        
        List<UISelectable> popUpItems = new List<UISelectable>();
        popUpItems.AddRange(sellCountButtons);
        popUpItems.AddRange(confirmButtons);

        // 売却画面と同じロジックコントローラーを初期化
        _amountSelectController = new ItemAmountSelectController(_confirmationPopup, _confirmationText, popUpItems, _SellCountButton);
        
        transform.gameObject.SetActive(false);
        _contentRect.anchoredPosition = new Vector2(-121.3f, 50f);
    }

    private void SetShopData()
    {
        PlayerCoinText.text = playerStatus.Coin.ToString();
        _underText.text = "何を買いますか？";

        foreach (Transform child in ProductList.transform) child.gameObject.SetActive(false);

        int itemCount = _currentShop.shopType == ShopType.Item ? _currentShop.itemList.Count : _currentShop.equipmentList.Count;

        for (int i = 0; i < itemCount; i++)
        {
            ProductList.transform.GetChild(i).gameObject.SetActive(true);
            
            string name = "";
            int pieces = 0;
            int price = 0;

            // タイプによって取得先を切り替える
            if (_currentShop.shopType == ShopType.Item)
            {
                name = _currentShop.itemList[i].itemData.Name;
                pieces = _currentShop.itemList[i].pieces;
                price = _currentShop.itemList[i].itemData.Price;
            }
            else
            {
                name = _currentShop.equipmentList[i].equipmentData.Name;
                pieces = _currentShop.equipmentList[i].pieces;
                price = _currentShop.equipmentList[i].equipmentData.Price;
            }
            
            ProductList.transform.GetChild(i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = name;
            ProductList.transform.GetChild(i).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = pieces.ToString();
            ProductList.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = price.ToString();
        }

        _choiceSelectables = ProductList.GetComponentsInChildren<UISelectable>(true)
            .Where(item => item.gameObject.activeSelf)
            .ToList();
    }
    
    public override void Show()
    {
        base.Show();
        UIManager.Instance.PlayerStop = true;
        SetShopData();
    }
    
    public async UniTask StartSelectItem(ShopInventory shopInventory)
    {
        _currentShop = shopInventory;
        Show();

        while (!_cts.IsCancellationRequested)
        {
            await UniTask.Yield();
            int selectNumber = await _selectInput.WaitForSelection(_choiceSelectables, _cts.Token, OnSelectionChanged , _lastIndex);

            if (selectNumber < 0) break; // キャンセルされたらループを抜ける

            _lastIndex = selectNumber; 

            // アイテム情報の取得
            string itemName = "";
            int itemPrice = 0;
            int stock = 0;

            if (_currentShop.shopType == ShopType.Item)
            {
                itemName = _currentShop.itemList[selectNumber].itemData.Name;
                itemPrice = _currentShop.itemList[selectNumber].itemData.Price;
                stock = _currentShop.itemList[selectNumber].pieces;
            }
            else
            {
                itemName = _currentShop.equipmentList[selectNumber].equipmentData.Name;
                itemPrice = _currentShop.equipmentList[selectNumber].equipmentData.Price;
                stock = _currentShop.equipmentList[selectNumber].pieces;
            }

            // 【重要】買える最大個数を計算する（所持金と在庫の少ない方）
            int maxAffordable = itemPrice > 0 ? playerStatus.Coin / itemPrice : stock;
            int maxBuyAmount = Mathf.Min(stock, maxAffordable);

            if (maxBuyAmount <= 0)
            {
                _underText.text = "コインが足りないか、在庫がありません";
                continue;
            }

            // 装備屋の場合は基本的に1個ずつ買う仕様にする
            if (_currentShop.shopType == ShopType.Equipment) maxBuyAmount = 1;

            _underText.text = "";

            // 売却画面で作った個数選択UIを呼び出す（最大購入可能数を渡す）
            int buyAmount = await _amountSelectController.WaitForConfirmationAsync(maxBuyAmount, _cts.Token);

            if (buyAmount <= 0) 
            {
                _underText.text = "何を買いますか？";
                continue; // キャンセルされたら戻る
            }
            
            // 購入処理の実行
            int totalPrice = itemPrice * buyAmount;
            playerStatus.DownCoin(totalPrice);

            if (_currentShop.shopType == ShopType.Item)
            {
                playerItemInventory.AddItem(_currentShop.itemList[selectNumber].itemData, buyAmount);
                _currentShop.itemList[selectNumber].pieces -= buyAmount;
            }
            else
            {
                playerEquipmentInventory.AddEquipment(_currentShop.equipmentList[selectNumber].equipmentData);
                _currentShop.equipmentList[selectNumber].pieces -= buyAmount;
            }

            SetShopData();
        }

        Hide();
    }
    
    private void OnSelectionChanged(int index)
    {
        if(_currentIndex != index) _underText.text = "何を買いますか？";
        
        if (_currentShop.shopType == ShopType.Item)
        {
            ItemImage.sprite = itemImageData.GetSprite(_currentShop.itemList[index].itemData.Id);
            _productDescription.text = _currentShop.itemList[index].itemData.Explanation;
        }
        else
        {
            ItemImage.sprite = itemImageData.GetSprite(_currentShop.equipmentList[index].equipmentData.Id);
            _productDescription.text = _currentShop.equipmentList[index].equipmentData.Explanation.ToString();
        }

        Scroll(index);
    }
    
    private void Scroll(int index)
    {
        if (_viewportRect == null || _contentRect == null || _choiceSelectables.Count <= index) return;

        if (index == 0)
        {
            _contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, 50f);
            _currentIndex = index;
            return;
        }
        
        Canvas.ForceUpdateCanvases();
        RectTransform selectedRect = _choiceSelectables[index].GetComponent<RectTransform>();
        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];
        
        _viewportRect.GetWorldCorners(viewportCorners);
        selectedRect.GetWorldCorners(itemCorners);

        float itemBottom = itemCorners[0].y;
        float itemTop = itemCorners[1].y;
        float viewportTop    = viewportCorners[1].y - topPadding;
        float viewportBottom = viewportCorners[0].y + bottomPadding;

        if (itemTop > viewportTop)
        {
            _contentRect.position -= new Vector3(0, itemTop - viewportTop, 0);
        }
        else if (itemBottom < viewportBottom)
        {
            _contentRect.position += new Vector3(0, viewportBottom - itemBottom, 0);
        }
        
        _currentIndex = index;
    }
    
    public override void Hide()
    {
        base.Hide();
        UIManager.Instance.SelectShopActionView.StartSelectAction(_currentShop).Forget();
    }
}