using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks; 
using UnityEngine;
using UnityEngine.UI;

public class EquipmentShopView : UIViewBase
{
    
    [SerializeField] private Image ItemImage;
    [SerializeField] private GameObject ProductList;
    
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private SelectInput _selectInput = new SelectInput();
    
    private List<UISelectable> _choiceSelectables = new List<UISelectable>();


    public async UniTask StartSelectItem()
    {
        
        await _selectInput.WaitForSelection(_choiceSelectables, _cts.Token);
    }

}
