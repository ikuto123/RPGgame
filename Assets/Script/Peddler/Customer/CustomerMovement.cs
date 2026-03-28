using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    private Vector3 destination;

    public void Move()
    {
        Debug.Log(destination);
        transform.position = Vector3.MoveTowards(transform.position, destination, walkSpeed * Time.deltaTime);
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        Vector3 direction = (destination - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}