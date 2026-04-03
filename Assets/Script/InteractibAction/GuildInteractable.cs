using UnityEngine;

public class GuildInteractable : MonoBehaviour , IInteractable
{
    public void OnInteract(PlayerController player)
    {
        Debug.Log("クエストの開始");
    }

    public string ShowInteractionText()
    {
        return "クエストを見る [E]";
    }
    
    public void OnFocus()
    {
        
    }
}
