using UnityEngine;

public class PlayerTransfotmInHouse : MonoBehaviour
{
    [SerializeField] private Transform playerHouseExteriorSpawnPoint;
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            other.gameObject.transform.position = playerHouseExteriorSpawnPoint.position;
        }
    }
}
