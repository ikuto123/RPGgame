using System; 
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogueGraphView : GraphView
{
    // ノードが選択されたときにインスペクターへ通知するイベント
    public Action<DialogueNode> OnNodeSelected;

    public DialogueGraphView()
    {
        // ズームやドラッグ操作の有効化
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        // 背景グリッドの追加
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    }

    // ノード選択時の処理
    public override void AddToSelection(ISelectable selectable)
    {
        base.AddToSelection(selectable);
        if (selectable is DialogueNode node)
        {
            OnNodeSelected?.Invoke(node);
        }
    }

    // 選択解除時の処理
    public override void ClearSelection()
    {
        base.ClearSelection();
        OnNodeSelected?.Invoke(null); 
    }

    // ポート接続のルール（自分自身や同じ向きのポートには繋げない）
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => 
            endPort.direction != startPort.direction && 
            endPort.node != startPort.node).ToList();
    }
    
    // 右クリックメニューの構築
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // 通常のCreate Nodeなどを残す場合はbaseを呼ぶ
        base.BuildContextualMenu(evt);
        
        // セパレーター（区切り線）があれば綺麗だが必須ではない
        evt.menu.AppendSeparator();

        // 3種類のノード作成メニューを追加
        evt.menu.AppendAction("Create Dialogue Node", (a) => CreateDialogueNode("Dialogue Node"));
        evt.menu.AppendAction("Create Branch Node", (a) => CreateBranchNode("Branch Logic"));
        evt.menu.AppendAction("Create Set Variable Node", (a) => CreateSetVariableNode("Set Variable"));
    }

    // ---------------------------------------------------------
    // ノード作成メソッド群
    // ---------------------------------------------------------

    // 1. 会話ノードの作成
    public DialogueNode CreateDialogueNode(string nodeName, ConversationEventSO eventData = null)
    {
        var node = CreateBaseNode(nodeName, NodeType.Dialogue);
        node.EventData = eventData;

        // 入力ポート
        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // 出力ポート追加ボタン
        var button = new Button(() => { AddChoicePort(node); }) { text = "Add Choice" };
        node.titleContainer.Add(button);

        // SOを表示するフィールド (VisualElementとしてノード内に表示)
        // ※ただしインスペクター側で編集するならここは表示専用か簡易表示にする
        /*
        var objectField = new ObjectField("Event Data")
        {
            objectType = typeof(ConversationEventSO),
            allowSceneObjects = false,
            value = eventData
        };
        objectField.RegisterValueChangedCallback(evt => {
            node.EventData = evt.newValue as ConversationEventSO;
        });
        node.extensionContainer.Add(objectField);
        */

        RefreshNode(node);
        return node;
    }

    // 2. 分岐ノードの作成
    public DialogueNode CreateBranchNode(string nodeName)
    {
        var node = CreateBaseNode(nodeName, NodeType.Branch);
        
        // 入力
        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // 条件追加ボタン
        var button = new Button(() => { AddChoicePort(node, "0"); }) { text = "Add Condition" };
        node.titleContainer.Add(button);
        
        // 色変更（緑系）
        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.1f, 0.35f, 0.1f));

        RefreshNode(node);
        return node;
    }
    
    // 3. 変数操作ノードの作成
    public DialogueNode CreateSetVariableNode(string nodeName)
    {
        var node = CreateBaseNode(nodeName, NodeType.SetVariable);

        // 入力
        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // 出力（次は必ず1つ）
        var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Single);
        outputPort.portName = "Next";
        node.outputContainer.Add(outputPort);

        // 色変更（紫系）
        node.titleContainer.style.backgroundColor = new StyleColor(new Color(0.4f, 0.1f, 0.4f));

        RefreshNode(node);
        return node;
    }

    // 共通のベースノード作成処理
    private DialogueNode CreateBaseNode(string title, NodeType type)
    {
        var node = new DialogueNode
        {
            title = title,
            GUID = Guid.NewGuid().ToString(),
            Type = type
        };
        // デフォルト位置
        node.SetPosition(new Rect(100, 200, 250, 150));
        
        AddElement(node);
        return node;
    }

    // ---------------------------------------------------------
    // ヘルパーメソッド
    // ---------------------------------------------------------

    // 出力ポート（選択肢）の追加
    public Port AddChoicePort(DialogueNode node, string name = "Next")
    {
        var port = GeneratePort(node, Direction.Output);
        
        // ポート名のラベルを消して、代わりにテキストフィールドを入れるか、
        // 単にラベルの横にテキストフィールドを置く構成
        var oldLabel = port.contentContainer.Q<Label>("type");
        if(oldLabel != null) port.contentContainer.Remove(oldLabel);

        var textField = new TextField { name = string.Empty, value = name };
        textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
        textField.style.minWidth = 60; 
        
        // テキストフィールドをポートの表示領域に追加
        port.contentContainer.Add(new Label("  ")); // 少し隙間
        port.contentContainer.Add(textField);
        
        // 重要: ポート名自体も初期化
        port.portName = name;

        node.outputContainer.Add(port);
        node.RefreshPorts();
        node.RefreshExpandedState();
        return port; 
    }

    // ポート生成の基本処理
    private Port GeneratePort(DialogueNode node, Direction dir, Port.Capacity cap = Port.Capacity.Single)
    {
        // 型は任意のものを指定可能だが、ここでは汎用的にfloatまたはboolとしておく
        // 接続制限を厳密にするならカスタムクラス型を指定する
        return node.InstantiatePort(Orientation.Horizontal, dir, cap, typeof(float));
    }
    
    private void RefreshNode(DialogueNode node)
    {
        node.RefreshExpandedState();
        node.RefreshPorts();
    }
}