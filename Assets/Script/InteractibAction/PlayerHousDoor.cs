using UnityEngine;

public class PlayerHousDoor : MonoBehaviour , IInteractable
{
    
    [SerializeField] private Transform playerHouseInteriorSpawnPoint;
    public void OnInteract(PlayerController player)
    {
        player.gameObject.transform.position = playerHouseInteriorSpawnPoint.position;
    }

    public string ShowInteractionText()
    {
        return "入る [E]";
    }
    
    public void OnFocus()
    {
        Debug.Log("NPCに注目しました。");
    }
}
