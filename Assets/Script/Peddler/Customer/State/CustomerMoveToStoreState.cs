using UnityEngine;

public class CustomerMoveToStoreState : CustomerBaseState
{
    public CustomerMoveToStoreState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
    public override void Enter() 
    { 
        Debug.Log("State: MoveToStore");
        _customer.Movement.SetDestination(_customer.Data.leavePos);
    }

    public override void Update()
    {
        _customer.Data.timer += Time.deltaTime;
        if(_customer.Data.timer > _customer.Data.moveToStoreTime)
        {
            _stateMachine.ChangeState(_customer.WaitAtStoreState);
        }
    }

    public override void PhysicsUpdate()
    {
        _customer.Movement.Move();
    }

    public override void Exit() {
        _customer.Data.timer = 0f;
    }
}
