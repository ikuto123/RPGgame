using UnityEngine;

[RequireComponent(typeof(DialogueRunner))]
public class NPCInteractable : MonoBehaviour, IInteractable
{
    private DialogueRunner _runner;

    private void Start()
    {
        _runner = GetComponent<DialogueRunner>();
    }

    public void OnInteract(PlayerController player)
    {
        //プレイヤーを動けなくする処理などは、Runnerを経由してUI側が自動でやってくれます
        _runner.StartDialogue();
    }

    public string ShowInteractionText()
    {
        return "話す [E]";
    }
    
    public void OnFocus()
    {
        Debug.Log("NPCに注目しました。");
    }
    
}