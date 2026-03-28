using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController _player;
    protected PlayerStateMachine _stateMachine;

    public PlayerBaseState(PlayerController player, PlayerStateMachine stateMachine)
    {
        _player = player;
        _stateMachine = stateMachine;
    }

    public abstract void Enter();  
    public abstract void Update(); 
    public abstract void PhysicsUpdate(); 
    public abstract void Exit();   
}