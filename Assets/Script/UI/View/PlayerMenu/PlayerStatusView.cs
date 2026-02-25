using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusView : UIViewBase
{
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI playerMPText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI playerEXPText;
    [SerializeField] private TextMeshProUGUI playerAttackText;
    [SerializeField] private TextMeshProUGUI playerDefenseText;
    [SerializeField] private Button ShowItemButton;
    
    public override void Show()
    {
        base.Show();
        ShowItemButton.onClick.AddListener(ShowItemView);
        UIManager.Instance.PlayerStop = true;
    }
    
    public void UpdateStatus(int hp, int mp, int level, int exp, int attack, int defense)
    {
        playerHPText.text = "HP: " + hp.ToString();
        playerMPText.text = "MP: " + mp.ToString();
        playerLevelText.text = "Level: " + level.ToString();
        playerEXPText.text = "EXP: " + exp.ToString();
        playerAttackText.text = "Attack: " + attack.ToString();
        playerDefenseText.text = "Defense: " + defense.ToString();
    }
    
    public override void Hide()
    {
        base.Hide();
        ShowItemButton.onClick.RemoveListener(ShowItemView);
        UIManager.Instance.PlayerStop = false;
    }

    private void ShowItemView()
    {
        Hide();
        UIManager.Instance.PlayerItemView.Show();
    }
}
