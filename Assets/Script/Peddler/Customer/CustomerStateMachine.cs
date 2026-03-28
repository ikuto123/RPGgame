using UnityEngine;

public class CustomerStateMachine
{
    public CustomerBaseState CurrentState{ get; private set; }

    public void Initialize(CustomerBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter(); 
    }

    //ó‘Ô‚ğØ‚è‘Ö‚¦‚é
    public void ChangeState(CustomerBaseState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}