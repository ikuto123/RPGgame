using UnityEngine;

public class PCInputProvider : MonoBehaviour, IInputProvider
{
    public PlayerInputData GetInput()
    {
        if (UIManager.Instance != null && 
            UIManager.Instance.DialogueView != null && 
            UIManager.Instance.PlayerStop)
        {
            return new PlayerInputData(); //入力なしを返す
        }
        

        return new PlayerInputData
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            LookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")),
            IsSprint = Input.GetKey(KeyCode.LeftShift),
            IsJump = Input.GetKeyDown(KeyCode.Space),
            IsInteract = Input.GetKeyDown(KeyCode.E),
            IsMenuOpen = Input.GetKeyDown(KeyCode.M)
        };
    }
}