using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum PortraitPosition { Left, Right, None }

[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
    public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();

    // ▼▼▼ 修正箇所: リストの0番目ではなく、論理的な「開始ノード」を探す ▼▼▼
    public DialogueNodeData GetFirstNode()
    {
        // ノードが空ならnull
        if (DialogueNodeData.Count == 0) return null;

        // 1. 「矢印の先（Target）」になっているノードのGUIDをすべて集める
        //    (＝誰かから繋がれているノードのリスト)
        var targetNodeGuids = new HashSet<string>(NodeLinks.Select(x => x.TargetNodeGUID));

        // 2. ノード一覧の中から、「矢印の先リスト」に含まれていないノードを探す
        //    (＝誰からも繋がれていない ＝ スタート地点)
        var rootNode = DialogueNodeData.Find(x => !targetNodeGuids.Contains(x.NodeGUID));

        // 3. 見つかればそれを返す。見つからなければ（ループしている場合など）保険で0番目を返す
        return rootNode ?? DialogueNodeData[0];
    }

    // ... GetNextNode, GetChoices はそのままでOK ...
    public DialogueNodeData GetNextNode(string currentGuid, string portName = null)
    {
        var link = NodeLinks.FirstOrDefault(x => 
            x.BaseNodeGUID == currentGuid && 
            (string.IsNullOrEmpty(portName) || x.PortName == portName)
        );

        if (link == null) return null;
        return DialogueNodeData.Find(x => x.NodeGUID == link.TargetNodeGUID);
    }

    public List<NodeLinkData> GetChoices(string currentGuid)
    {
        return NodeLinks.Where(x => x.BaseNodeGUID == currentGuid).ToList();
    }
}