using UnityEngine;

public class ShopInteratable : MonoBehaviour , IInteractable
{
    public void OnFocus()
    {
        
    }

    public void OnInteract(PlayerController player)
    {
        
    }

    public string ShowInteractionText()
    {
        return "E ショップ";   
    } 

}
