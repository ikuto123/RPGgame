using UnityEngine;

public class CustomerMoveToProductState : CustomerBaseState
{
    public CustomerMoveToProductState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
    public override void Enter() { 
        Debug.Log("State: MoveToProduct");
        _customer.Movement.SetDestination(_customer.Data.productPos);
    }

    public override void Update()
    {
        if(Vector3.Distance(_customer.transform.position, _customer.Data.productPos) > 0.01f)
        {
             _stateMachine.ChangeState(_customer.WaitAtProductState);
        }
    }

    public override void PhysicsUpdate()
    {
        _customer.Movement.Move();
    }

    public override void Exit() { }
}