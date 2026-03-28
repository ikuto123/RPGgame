using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform FollowTarget;      
    [SerializeField] float _distance;         

    [Header("Rotation Settings")]
    public float Sensitivity = 2f;      //マウス感度
    public Vector2 PitchLimit = new Vector2(-10f, 60f); 

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }
    public void OnLateUpdate(Vector2 lookInput)
    {
        if (FollowTarget == null) return;

        _yaw += lookInput.x * Sensitivity;
        _pitch -= lookInput.y * Sensitivity;
        _pitch = Mathf.Clamp(_pitch, PitchLimit.x, PitchLimit.y);

        Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        
        Vector3 targetPosition = FollowTarget.position - (targetRotation * Vector3.forward * _distance);

        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }
}
