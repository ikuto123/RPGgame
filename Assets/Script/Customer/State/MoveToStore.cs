using UnityEngine;

public class MoveToStore : CustomerBaseState
{
    public MoveToStore(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
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
