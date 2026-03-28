using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() { Debug.Log("State: Idle"); }
    public override void PhysicsUpdate() { }

    public override void Update()
    {

        if (_player.InputData.MoveInput != Vector2.zero)
        {
            _stateMachine.ChangeState(_player.WalkState);
        }
        if (_player.InputData.IsJump && _player.IsGrounded())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }
    
    public override void Exit() { }

}
