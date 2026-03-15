using UnityEngine;

public abstract class CustomerBaseState
{
    protected CustomerController _customer;
    protected CustomerStateMachine _stateMachine;

    public CustomerBaseState(CustomerController customer, CustomerStateMachine stateMachine)
    {
        _customer = customer;
        _stateMachine = stateMachine;
    }

    public abstract void Enter();  
    public abstract void Update(); 
    public abstract void PhysicsUpdate(); 
    public abstract void Exit();   
}