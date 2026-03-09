using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShopInteratable : MonoBehaviour , IInteractable
{
    public void OnFocus()
    {
        
    }

    public void OnInteract(PlayerController player)
    {
        UIManager.Instance.SelectShopActionView.StartSelectAction().Forget();

    }

    public string ShowInteractionText()
    {
        return "E ショップ";   
    } 

}
