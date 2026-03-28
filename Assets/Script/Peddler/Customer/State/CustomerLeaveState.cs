using UnityEngine;

public class CustomerLeaveState : CustomerBaseState
{
    public CustomerLeaveState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine) { }

    public override void Enter() 
    { 
        Debug.Log("State: Leave"); 
        _customer.Movement.SetDestination(_customer.Data.leavePos);
    }

    public override void Update()
    {
        //画面外に出たらプールに戻す処理を追記予定
    }

    public override void PhysicsUpdate()
    {
        _customer.Movement.Move();
    }

    public override void Exit() 
    { 
        
    }
}