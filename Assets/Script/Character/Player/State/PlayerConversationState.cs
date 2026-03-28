using UnityEngine;

public class PlayerConversationState : PlayerBaseState
{
    public PlayerConversationState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }
    
    public override void Enter()
    {
        _player.Rb.linearVelocity = Vector3.zero; 
        
        // アニメーションをIdleにするならここでセット
    }

    public override void Update()
    {

    }

    public override void PhysicsUpdate() { }
    public override void Exit() { }
}
