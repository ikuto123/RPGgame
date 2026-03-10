using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks; 
using System.Threading;
using UnityEditor.Search;

public class SelectShopActionView : UIViewBase
{
    [SerializeField] private GameObject shopActionButtons;

    private List<UISelectable> _shopActionSelectables = new List<UISelectable>();
    
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();
    

    private void Awake()
    {
        _shopActionSelectables = shopActionButtons.GetComponentsInChildren<UISelectable>(true).ToList();
        Hide();
    }

    public override void Show()
    {
        base.Show();
        UIManager.Instance.PlayerStop = true;

    }
    
    public override void Hide()
    {
        base.Hide();
        if(UIManager.Instance == null) return;
        UIManager.Instance.PlayerStop = false;
    }
    
    public async UniTask StartSelectAction(ShopInventory shopInventory)
    {
        Show();

        int selectNumber = await _selectInput.WaitForSelection(_shopActionSelectables, _cts.Token);

        switch (selectNumber)
        {
            case 0 ://買う
                transform.gameObject.SetActive(false);
                UIManager.Instance.EquipmentShopView.StartSelectItem(shopInventory).Forget();
                break;
            case 1 ://売る
                transform.gameObject.SetActive(false);
                UIManager.Instance.PlayerItemView.StartSelectPlayerInventory(PlayerItemMenuMode.Sell).Forget();
                break;
            default:
                Hide();
                break;
        }
        
    }
}
