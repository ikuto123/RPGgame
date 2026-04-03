using TMPro;
using UnityEngine;

public class PlayerStatusView : UIViewBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _playerHPText;
    [SerializeField] private TextMeshProUGUI _playerMPText;
    [SerializeField] private TextMeshProUGUI _playerAttackText;
    [SerializeField] private TextMeshProUGUI _playerDefanseText;
    [SerializeField] private TextMeshProUGUI _playerlevelText;
    [SerializeField] private TextMeshProUGUI _playerCoinText;
    
    [SerializeField] private PlayerStatus playerStatus;

    void Awake()
    {
        UpdateStatus();
    }
    
    public void UpdateStatus()
    {
        _playerHPText.text = $"HP: {playerStatus.currentHP} / {playerStatus.Hp}";
        _playerMPText.text = $"MP: {playerStatus.currentMP} / {playerStatus.Mp}";
        _playerAttackText.text = $"Attack: {playerStatus.TotalAttack}";
        _playerDefanseText.text = $"Defense: {playerStatus.TotalDefense}";
        _playerlevelText.text = $"Level: {playerStatus.Level}";
        _playerCoinText.text = playerStatus.Coin.ToString();
    }
    
    public void UpdatePreviewStatus(EquipmentData currentEquip, EquipmentData newEquip)
    {
        int currentAtk = currentEquip != null ? currentEquip.Attack : 0;
        int currentDef = currentEquip != null ? currentEquip.Defense : 0;

        int newAtk = newEquip != null ? newEquip.Attack : 0;
        int newDef = newEquip != null ? newEquip.Defense : 0;

        int atkDiff = newAtk - currentAtk;
        int defDiff = newDef - currentDef;

        // ▼ここを変更
        _playerAttackText.text = FormatStatPreview("Attack", playerStatus.TotalAttack, atkDiff);
        _playerDefanseText.text = FormatStatPreview("Defense", playerStatus.TotalDefense, defDiff);
    }
    
    private string FormatStatPreview(string statName, int currentTotal, int diff)
    {
        if (diff == 0) return $"{statName}: {currentTotal}"; // 変化なしならそのまま

        int newTotal = currentTotal + diff;
        
        string colorTag = diff > 0 ? "<color=#00FF00>" : "<color=#FF0000>";
        
        return $"{statName}: {currentTotal} {colorTag}>> {newTotal}</color>";
    }

}
