using UnityEngine;

public class CustomerWaitAtStoreState :  CustomerBaseState
{
    public CustomerWaitAtStoreState(CustomerController customer, CustomerStateMachine stateMachine) : base(customer, stateMachine) { }

    public override void Enter() 
    { 
        Debug.Log("State: WaitAtStore"); 
    }

    public override void Update()
    {
        //‚±‚±‚ÅƒŠƒXƒg”äŠrˆ—‚ð’Ç‹L
        _customer.Data.timer += Time.deltaTime;
        if(_customer.Data.timer > _customer.Data.waitAtStoreTime)
        {
            _stateMachine.ChangeState(_customer.MoveToProductState);
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
