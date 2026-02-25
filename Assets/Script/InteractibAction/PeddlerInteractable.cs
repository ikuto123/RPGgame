using UnityEngine;

public class PeddlerInteractable : MonoBehaviour , IInteractable
{

    public void OnInteract(PlayerController player)
    {
        Debug.Log("商売をする処理を開始");
    }

    public string ShowInteractionText()
    {
        return "商売をする [E]";
    }
    
    public void OnFocus()
    {
        
    }
}
