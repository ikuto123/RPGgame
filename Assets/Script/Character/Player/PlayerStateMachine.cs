using UnityEngine;

public class PlayerStateMachine
{
    public PlayerBaseState CurrentState { get; private set; }

    public void Initialize(PlayerBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter(); 
    }

    //状態を切り替える
    public void ChangeState(PlayerBaseState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
