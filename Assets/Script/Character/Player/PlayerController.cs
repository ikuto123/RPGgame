using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(IInputProvider))] 
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    public float JumpForce = 5f;
    public float TurnSpeed = 15f; 

    [Header("References")]
    public PlayerCamera PlayerCamera; 
    public Transform GroundCheckPos;   
    
    [Header("Ground Check")]
    [SerializeField] private float _groundCheckRadius;
    [SerializeField] private LayerMask _groundLayer;
    
    public Rigidbody Rb { get; private set; }
    private IInputProvider _inputProvider; 
    
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerConversationState ConversationState { get; private set; }

    public PlayerInputData InputData { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        _inputProvider = GetComponent<IInputProvider>(); 

        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine);
        WalkState = new PlayerWalkState(this, StateMachine);
        RunState = new PlayerRunState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine); 
        ConversationState = new PlayerConversationState(this, StateMachine);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        if (_inputProvider != null)
        {
            InputData = _inputProvider.GetInput();
        }

        StateMachine.CurrentState.Update();
    }

    private void LateUpdate()
    {
        if (PlayerCamera != null)
        {
            PlayerCamera.OnLateUpdate(InputData.LookInput);
        }
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    public void Move(float speed)
    {
        if (InputData.MoveInput == Vector2.zero) return;
        if (PlayerCamera == null) return;
        
        Vector3 cameraForward = PlayerCamera.transform.forward;
        Vector3 cameraRight = PlayerCamera.transform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDir = (cameraForward * InputData.MoveInput.y + cameraRight * InputData.MoveInput.x).normalized;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);
        }

        Rb.MovePosition(transform.position + moveDir * speed * Time.fixedDeltaTime);
    }
    
    public bool IsGrounded()
    {
        return Physics.CheckSphere(GroundCheckPos.position, _groundCheckRadius, _groundLayer);
    }
    
    private void OnDrawGizmos()
    {
        if (GroundCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GroundCheckPos.position, _groundCheckRadius);
        }
    }
}