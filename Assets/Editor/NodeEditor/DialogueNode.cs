using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class DialogueNode : Node
{
    public string GUID;
    
    public ConversationEventSO EventData;
    
    public NodeType Type = NodeType.Dialogue;
    
    public string TargetVariable = "NewVariable"; 
    public VariableOpType OperationType = VariableOpType.Set; 
    public int TargetValue = 1; 
}