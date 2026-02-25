using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemView : UIViewBase
{
    [SerializeField] private Button ShowPlayerStatusButton;
    [SerializeField] private GameObject ItemBox;
    [SerializeField] private ItemImageData itemImageData;

    private Image[] itemImages;
    private TextMeshProUGUI[] itemTexts;

    private void Start()
    {
        itemImages = new Image[ItemBox.transform.childCount];
        itemTexts = new TextMeshProUGUI[ItemBox.transform.childCount];
        
        for(int i = 0; i < ItemBox.transform.childCount; i++)
        {
            var itemSlot = ItemBox.transform.GetChild(i);
            itemImages[i] = itemSlot.GetComponent<Image>();
            itemTexts[i] = itemSlot.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    public override void Show()
    {
        base.Show();
        ShowPlayerStatusButton.onClick.AddListener(ShowPlayerStatusView);
        UIManager.Instance.PlayerStop = true;
    }

    public override void Hide()
    {
        base.Hide();
        ShowPlayerStatusButton.onClick.RemoveListener(ShowPlayerStatusView);
        UIManager.Instance.PlayerStop = false;
    }

    private void ShowPlayerStatusView()
    {
        Hide();
        UIManager.Instance.PlayerStatusView.Show();
    }
    
    public void UpdateItemBox(List<InventorySlot> inventory)
    {
        //既存のアイテム表示をクリア
        foreach (Transform child in ItemBox.transform)
        {
            child.gameObject.SetActive(false);
        }
        Debug.Log("持ち物" + inventory.Count);
        
        for(int i = 0; i < inventory.Count; i++)
        {
            itemImages[i].sprite = itemImageData.GetSprite(inventory[i].itemData.Id);
            itemTexts[i].text = inventory[i].quantity.ToString();
            itemImages[i].gameObject.SetActive(true);
        }
    }
    
}
