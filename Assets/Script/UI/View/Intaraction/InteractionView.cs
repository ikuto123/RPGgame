using UnityEngine;
using TMPro; 

public class InteractionView : UIViewBase //親がUIの表示・非表示を持つ
{
    [SerializeField] private TextMeshProUGUI _promptText;

    public void SetPromptText(string text)
    {
        _promptText.text = text;
    }
}