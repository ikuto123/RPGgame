using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CustomerController : MonoBehaviour
{            
    public CustomerStateMachine StateMachine { get; private set;} 
    public CustomerMoveToStoreState MoveToStoreState { get; private set; }
    public CustomerWaitAtStoreState WaitAtStoreState { get; private set; }
    public CustomerMoveToProductState MoveToProductState{ get; private set; }
    public CustomerWaitAtProductState WaitAtProductState{ get; private set; }
    public CustomerLeaveState LeaveState { get; private set; }

    public CustomerData Data;

    [HideInInspector] public CustomerMovement Movement;

    private void Awake()
    {
        StateMachine = new CustomerStateMachine();
        MoveToStoreState = new CustomerMoveToStoreState(this, StateMachine);
        WaitAtStoreState = new CustomerWaitAtStoreState(this, StateMachine);
        MoveToProductState = new CustomerMoveToProductState(this, StateMachine);
        WaitAtProductState = new CustomerWaitAtProductState(this, StateMachine);
        LeaveState = new CustomerLeaveState(this, StateMachine);
        
        Data = new CustomerData();
        Movement = GetComponent<CustomerMovement>();

        Debug.Log("Initialize Customer");
    }
    private void Start()
    {
        StateMachine.Initialize(MoveToStoreState);
    }


    private void Update()
    {
        StateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }    
}
