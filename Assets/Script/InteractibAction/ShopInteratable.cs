using UnityEngine;

public class ShopInteratable : MonoBehaviour , IInteractable
{
    public void OnFocus()
    {
        
    }

    public void OnInteract(PlayerController player)
    {
        UIManager.Instance.EquipmentShopView.Show();
        UIManager.Instance.EquipmentShopView.StartSelectItem();

    }

    public string ShowInteractionText()
    {
        return "E ショップ";   
    } 

}
