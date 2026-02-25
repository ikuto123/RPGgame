using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

public static class GraphSaveUtility
{
public static void SaveGraph(string fileName, DialogueGraphView graphView, bool overwrite)
    {
        // フォルダ確認
        if (!AssetDatabase.IsValidFolder("Assets/Resources/conversationNodes")) 
            AssetDatabase.CreateFolder("Assets/Resources","conversationNodes");

        string finalFileName = fileName;
        string path = $"Assets/Resources/conversationNodes/{finalFileName}.asset";
        
        if (!overwrite)
        {
            int counter = 1;
            while (AssetDatabase.LoadAssetAtPath<DialogueContainer>(path) != null)
            {
                finalFileName = $"{fileName}_{counter}";
                path = $"Assets/Resources/conversationNodes/{finalFileName}.asset";
                counter++;
            }
        }

        DialogueContainer container = AssetDatabase.LoadAssetAtPath<DialogueContainer>(path);
        
        if (container == null)
        {
            // 新規作成
            container = ScriptableObject.CreateInstance<DialogueContainer>();
            AssetDatabase.CreateAsset(container, path);
        }
        else
        {
            // 既存ファイルをクリアして再利用（これで参照切れを防ぐ）
            container.NodeLinks.Clear();
            container.DialogueNodeData.Clear();
        }

        //リンクの保存
        var connectedPorts = graphView.edges.ToList();
        foreach (var edge in connectedPorts)
        {
            var outputNode = edge.output.node as DialogueNode;
            var inputNode = edge.input.node as DialogueNode;
            
            container.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGUID = outputNode.GUID,
                PortName = edge.output.portName,
                TargetNodeGUID = inputNode.GUID
            });
        }

        // 4. ノードの保存
        foreach (var node in graphView.nodes.ToList().Cast<DialogueNode>())
        {
            container.DialogueNodeData.Add(new DialogueNodeData
            {
                NodeGUID = node.GUID,
                EventData = node.EventData, 
                NodePosition = node.GetPosition().position,
                Type = node.Type,
                TargetVariable = node.TargetVariable,
                OperationType = node.OperationType,
                TargetValue = node.TargetValue
            });
        }

        EditorUtility.SetDirty(container); 
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string message = overwrite ? $"Updated: {finalFileName}" : $"Saved as New: {finalFileName}";
    }

    public static void LoadGraph(string fileName, DialogueGraphView graphView)
    {
        string path = $"Assets/Resources/conversationNodes/{fileName}.asset";
        var container = AssetDatabase.LoadAssetAtPath<DialogueContainer>(path);
        if (container == null) return;
        ClearGraph(graphView);
        GenerateNodes(container, graphView);
    }
    
    private static void GenerateNodes(DialogueContainer container, DialogueGraphView graphView)
    {
         var createdNodes = new Dictionary<string, DialogueNode>();
         foreach (var nodeData in container.DialogueNodeData)
         {
             DialogueNode node = null;

             if (nodeData.Type == NodeType.Branch)
             {
                 node = graphView.CreateBranchNode("Branch");
             }
             else if (nodeData.Type == NodeType.SetVariable)
             {
                 node = graphView.CreateSetVariableNode("Set Var");
             }
             else
             {
                 node = graphView.CreateDialogueNode("Dialogue", nodeData.EventData); // 通常
             }
             node.GUID = nodeData.NodeGUID;
             node.Type = nodeData.Type;
             node.TargetVariable = nodeData.TargetVariable;
             node.OperationType = nodeData.OperationType;
             node.TargetValue = nodeData.TargetValue;
             
             node.SetPosition(new Rect(nodeData.NodePosition, new Vector2(250, 150)));
             createdNodes.Add(node.GUID, node);
         }
        foreach (var link in container.NodeLinks)
        {
            if (createdNodes.TryGetValue(link.BaseNodeGUID, out var outputNode) &&
                createdNodes.TryGetValue(link.TargetNodeGUID, out var targetNode))
            {
                Port outputPort;
                
                if (outputNode.Type == NodeType.SetVariable)
                {
                    outputPort = outputNode.outputContainer.Q<Port>(); 
                }
                else
                {
                    outputPort = graphView.AddChoicePort(outputNode, link.PortName);
                }
                var inputPort = targetNode.inputContainer.Q<Port>();
                var edge = outputPort.ConnectTo(inputPort);
                graphView.AddElement(edge);
            }
        }
    }

    private static void ClearGraph(DialogueGraphView graphView)
    {
        foreach (var node in graphView.nodes.ToList()) graphView.RemoveElement(node);
        foreach (var edge in graphView.edges.ToList()) graphView.RemoveElement(edge);
    }
}