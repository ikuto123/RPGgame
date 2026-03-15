using UnityEngine;

public class MoveToProductState : CustomerBaseState
{
    public MoveToProductState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
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