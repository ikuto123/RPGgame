using UnityEngine;

public class PlayerMenue : MonoBehaviour
{
    private PlayerStatus _status;
    private PlayerItemInventory _itemInventory;
    private IInputProvider _inputProvider;
    
    private void Start()
    {
        _inputProvider = GetComponent<IInputProvider>();
        _status = GetComponent<PlayerStatus>();
        _itemInventory = GetComponent<PlayerItemInventory>();

    }
    
    private void Update()
    {
        if (_inputProvider.GetInput().IsMenuOpen)//閉じる時の処理はビューに書いてある
        {
            UIManager.Instance.SetCursorState(true);
            

        }
    }
}
