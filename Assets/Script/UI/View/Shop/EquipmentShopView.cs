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
    
    [SerializeField] private ShopEquipment ShopItems;
    
    [SerializeField] private ItemImageData itemImageData;
    
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerEquipmentInventory playerItemInventory;
    
    [SerializeField] private TextMeshProUGUI _productDescription;
    
    [SerializeField] private GameObject _confirmationPopup;
    [SerializeField] private TextMeshProUGUI _confirmationText;
    [SerializeField] private GameObject _confirmButton;
    
    private List<UISelectable> _confirmButtons = new List<UISelectable>();
    
    Vector3[] corners = new Vector3[4];

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();
    
    private List<UISelectable> _choiceSelectables = new List<UISelectable>();

    private Action<int> _onItemSelected; 
    
    private int _currentIndex = 0;
    private float _scrollAmount = 50f; 

    private RectTransform[] ShopItemRects;
    
    private float topPadding = 70f;
    private float bottomPadding = 70f;
    private int _lastIndex = 0; 

    private void Awake()
    {
        _confirmButtons = _confirmButton.GetComponentsInChildren<UISelectable>(true).ToList();
        SetShopData();
        transform.gameObject.SetActive(false);
        
        _contentRect.anchoredPosition = new Vector2(-121.3f, 50f);
    }

    private void SetShopData()
    {
        PlayerCoinText.text = playerStatus.Coin.ToString();
        _confirmationText.text = "何を買いますか？";

        //文字を代入する
        for (int i = 0; i < ShopItems.equipmentList.Count; i++)
        {
            ProductList.transform.GetChild(i).gameObject.SetActive(true);
            
            ProductList.transform.GetChild(i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text
                = ShopItems.equipmentList[i].equipmentData.Name;
            
            ProductList.transform.GetChild(i).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text
                = ShopItems.equipmentList[i].pieces.ToString();
            
            ProductList.transform.GetChild(i).GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text
                = ShopItems.equipmentList[i].equipmentData.Price.ToString();
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
    
    public async UniTask StartSelectItem()
    {
        Show();
        while (!_cts.IsCancellationRequested)
        {
            await UniTask.Yield();
            int selectNumber = await _selectInput.WaitForSelection(_choiceSelectables, _cts.Token, OnSelectionChanged , _lastIndex);

            if (selectNumber < 0) break; //キャンセルされた

            var item = ShopItems.equipmentList[selectNumber];

            _lastIndex = selectNumber; 
            // 買えないなら「選択に戻る」
            if (item.pieces <= 0 || playerStatus.Coin < item.equipmentData.Price)
            {
                _confirmationText.text = "購入できません";
                continue;
            }

            if(!await Confirmation(item.equipmentData.Name,item.equipmentData.Price)) continue;

            
            // 買えるときだけ購入処理
            playerItemInventory.AddEquipment(item.equipmentData);
            playerStatus.DownCoin(item.equipmentData.Price);
            item.pieces--;
            

            SetShopData();

            continue;
        }
        Debug.Log("終了します");

        Hide();

    }
    
    private async UniTask<bool> Confirmation(string itemName, int price)
    {
        _confirmationText.text = $"{itemName} を {price} コインで購入しますか？";
        _confirmationPopup.SetActive(true);

        int selectedIndex = await _selectInput.WaitForSelection(_confirmButtons, _cts.Token);
        _confirmationText.text = "何を買いますか";
        _confirmationPopup.SetActive(false);

        return selectedIndex == 0; 
    }
    
    private void OnSelectionChanged(int index)
    {
        if(_currentIndex != index)　_confirmationText.text = "何を買いますか？";
        ItemImage.sprite = itemImageData.GetSprite(ShopItems.equipmentList[index].equipmentData.Id);
        _productDescription.text = ShopItems.equipmentList[index].equipmentData.Explanation.ToString();
        // スクロール処理を実行
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
        
        // ワールド座標を取得（[0]左下、[1]左上、[2]右上、[3]右下）
        _viewportRect.GetWorldCorners(viewportCorners);
        selectedRect.GetWorldCorners(itemCorners);

        // 比較のためにY座標（上下の位置）だけを取り出す
        float itemBottom = itemCorners[0].y;
        float itemTop = itemCorners[1].y;
        float viewportTop    = viewportCorners[1].y - topPadding;
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
        
        _currentIndex = index;
        
    }
    
    public override void Hide()
    {
        base.Hide();
        UIManager.Instance.SelectShopActionView.StartSelectAction().Forget();
    }

}
