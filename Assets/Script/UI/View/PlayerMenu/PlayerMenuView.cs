using UnityEngine;

public class PlayerMenuView : UIViewBase
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            base.Hide();
            UIManager.Instance.PlayerStatusView.Hide();
            UIManager.Instance.PlayerItemView.Hide();
            
            UIManager.Instance.SetCursorState(false);
        }
    }
}
