using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShopInteratable : MonoBehaviour , IInteractable
{
    public void OnFocus()
    {
        
    }

    public void OnInteract(PlayerController player)
    {
        var shopInventory = GetComponent<ShopInventory>();
        UIManager.Instance.SelectShopActionView.StartSelectAction(shopInventory).Forget();

    }

    public string ShowInteractionText()
    {
        return "E ショップ";   
    } 

}
