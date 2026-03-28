using UnityEngine;

public class CustomerWaitAtProductState : CustomerBaseState
{
    public CustomerWaitAtProductState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine){ }
    
    public override void Enter() { 
        Debug.Log("State: LeaveStore");
    }

    public override void Update()
    {
       _customer.Data.timer += Time.deltaTime;
       if(_customer.Data.timer > _customer.Data.waitAtProductTime)
       {
           _stateMachine.ChangeState(_customer.LeaveState);
       }
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void Exit() 
    {
        _customer.Data.timer = 0f;
    }
}