using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() { Debug.Log("State: Walk"); }

    public override void Update()
    {
        //止まったらIdleへ
        if (_player.InputData.MoveInput == Vector2.zero)
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
        
        //Shiftが押されたらRunへ
        else if (_player.InputData.IsSprint)
        {
            _stateMachine.ChangeState(_player.RunState);
        }
        else if (_player.InputData.IsJump && _player.IsGrounded())
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        //実際の移動処理
        _player.Move(_player.WalkSpeed);
    }
    
    public override void Exit() { }

}
