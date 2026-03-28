using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Linq; // これが必要です

public class DialogueRunner : MonoBehaviour
{
    [Header("Data")]
    public DialogueContainer DialogueGraph; 

    [Header("Events")]
    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;

    private DialogueNodeData _currentNode;

    public void StartDialogue()
    {
        if (DialogueGraph == null) return;

        // 最初のノードを取得
        var startNode = DialogueGraph.GetFirstNode();
        
        _currentNode = ProcessNodeLogic(startNode);

        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }

        OnDialogueStart?.Invoke();
        UIManager.Instance.DialogueView.Show(this);
        UpdateView();
    }

    public void Proceed(string choicePortName = null)
    {
        // UIから「次へ」と言われたら、次のノードを探す
        var nextNode = DialogueGraph.GetNextNode(_currentNode.NodeGUID, choicePortName);
        
        _currentNode = ProcessNodeLogic(nextNode);

        if (_currentNode != null)
        {
            UpdateView();
        }
        else
        {
            EndDialogue();
        }
    }

    private DialogueNodeData ProcessNodeLogic(DialogueNodeData node)
    {
        if (node == null) return null;

        if (node.Type == NodeType.Dialogue) return node;

        if (node.Type == NodeType.SetVariable)
        {
            if (DialogueStateManager.Instance != null)
            {
                if (node.OperationType == VariableOpType.Set)
                    DialogueStateManager.Instance.SetInt(node.TargetVariable, node.TargetValue);
                else
                    DialogueStateManager.Instance.AddInt(node.TargetVariable, node.TargetValue);
            }
            else
            {
                Debug.LogWarning("DialogueStateManagerがシーンにありません。変数は保存されませんでした。");
            }

            var nextNode = DialogueGraph.GetNextNode(node.NodeGUID, "Next");


            if (nextNode == null)
            {
                var link = DialogueGraph.NodeLinks.FirstOrDefault(x => x.BaseNodeGUID == node.NodeGUID);
                if (link != null)
                {
                    nextNode = DialogueGraph.DialogueNodeData.FirstOrDefault(x => x.NodeGUID == link.TargetNodeGUID);
                }
            }

            return ProcessNodeLogic(nextNode); 
        }

        if (node.Type == NodeType.Branch)
        {
            int currentValue = 0;
            if (DialogueStateManager.Instance != null)
            {
                currentValue = DialogueStateManager.Instance.GetInt(node.TargetVariable);
            }

            var choices = DialogueGraph.GetChoices(node.NodeGUID);
            
            var match = choices.FirstOrDefault(x => x.PortName == currentValue.ToString());

            if (match == null)
            {
                match = choices.FirstOrDefault(x => 
                    x.PortName.ToLower() == "default" || x.PortName.ToLower() == "else");
            }

            if (match != null)
            {
                var nextNode = DialogueGraph.GetNextNode(node.NodeGUID, match.PortName);
                return ProcessNodeLogic(nextNode);
            }
        }

        return null; 
    }

    private void UpdateView()
    {
        var choices = DialogueGraph.GetChoices(_currentNode.NodeGUID);
        UIManager.Instance.DialogueView.DisplayNode(_currentNode, choices).Forget();
    }

    private void EndDialogue()
    {
        OnDialogueEnd?.Invoke();
        UIManager.Instance.DialogueView.Hide();
    }
}