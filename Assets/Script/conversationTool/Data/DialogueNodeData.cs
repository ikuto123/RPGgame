using UnityEngine;


public enum NodeType
{
    Dialogue,
    Branch,
    SetVariable
}

public enum VariableOpType
{
    Set,
    Add
}

[System.Serializable]
public class DialogueNodeData
{
// ▼ 既存のフィールド（復活）
    public string NodeGUID;           // ノードID
    public string DialogueText;       // セリフ (直打ち用/キャッシュ用)
    public string SpeakerName;        // 話者名
    public Sprite Portrait;           // 立ち絵
    public PortraitPosition Position; // 立ち絵位置 (Enum定義が別途ある前提)
    
    public Vector2 NodePosition;      // エディタ上の座標
    public ConversationEventSO EventData; // 会話データ(SO)

    public NodeType Type;             // ノードの種類
    public string TargetVariable;     // 変数名
    public VariableOpType OperationType; // 操作タイプ (Set/Add)
    public int TargetValue;
    
}
