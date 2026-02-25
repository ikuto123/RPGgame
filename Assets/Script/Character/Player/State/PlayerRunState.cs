using UnityEngine;
public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() { Debug.Log("State: Run"); }

    public override void Update()
    {
        if (_player.InputData.MoveInput == Vector2.zero)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
        
        else if (!_player.InputData.IsSprint)
        {
            _stateMachine.ChangeState(_player.WalkState);
        }
        else if (_player.InputData.IsJump && _player.IsGrounded())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        // 実際の移動処理 (走り速度)
        _player.Move(_player.RunSpeed);
    }
    
    public override void Exit() { }

}