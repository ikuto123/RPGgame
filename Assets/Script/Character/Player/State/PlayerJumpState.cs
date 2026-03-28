using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("State: Jump");
        _player.Rb.AddForce(Vector3.up * _player.JumpForce, ForceMode.Impulse);
    }

    public override void Update()
    {
        if (_player.Rb.linearVelocity.y < 0.1f && _player.IsGrounded())
        {
            if (_player.InputData.MoveInput == Vector2.zero)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
            else
            {
                _stateMachine.ChangeState(_player.WalkState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        _player.Move(_player.WalkSpeed * 0.7f); 
    }
    
    public override void Exit()
    {
        // 着地した時の処理（着地音やエフェクトなど）があればここに書く
    }
    

}