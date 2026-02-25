using System.Collections.Generic;
using UnityEngine;
public enum RewardType
{
    Item,
    Equipment
}

[System.Serializable]
public class RewardEntry
{
    public RewardType Type;
    public ItemData Item;          // アイテムの場合
    public EquipmentData Equipment;// 装備の場合
    [Min(1)] public int Amount = 1;
}

[CreateAssetMenu(fileName = "ConversationEventSO", menuName = "Scriptable Objects/ConversationEventSO")]
public class ConversationEventSO : ScriptableObject
{

    [Header("会話の内容")]
    public string SpeakerName;     // 話し手の名前
    [TextArea]
    public string DialogueText;    // セリフ

    [Header("立ち絵の設定")]
    public Sprite LeftPortrait;    // 左側に表示する画像（変更しないならNoneでOK）
    public Sprite RightPortrait;   // 右側に表示する画像（変更しないならNoneでOK）
    
    
    public PortraitPosition SpeakerSide; // どっちがしゃべっているか
    
    [Header("Rewards")]
    public List<RewardEntry> Rewards = new List<RewardEntry>();
}
