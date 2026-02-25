using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Views")]
    public InteractionView InteractionView;
    public DialogueView DialogueView;
    public PlayerStatusView PlayerStatusView;
    public PlayerItemView PlayerItemView;
    public PlayerMenuView PlayerMenuView;
    public ItemRewardView ItemRewardView;
    public EquipmentShopView EquipmentShopView;

    public bool PlayerStop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeAllViews();
    }

    private void InitializeAllViews()
    {
        //InteractionViewがあれば初期化
        if (InteractionView != null)
        {
            InteractionView.Initialize();
            InteractionView.Hide();
        }

        // 他のUIも同様に...
    }

    // ショップを開いたときはカーソルを表示、閉じたら消すなどの制御用
    public void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}