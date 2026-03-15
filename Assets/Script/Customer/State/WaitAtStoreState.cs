using UnityEngine;

public class WaitAtStoreState : CustomerBaseState
{
    public WaitAtStoreState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
    public override void Enter() { Debug.Log("State: MoveToStore");}

    public override void Update()
    {
        //
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void Exit() { }
}