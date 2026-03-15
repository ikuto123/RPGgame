using UnityEngine;

public class LeaveState : CustomerBaseState
{
    public LeaveState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
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