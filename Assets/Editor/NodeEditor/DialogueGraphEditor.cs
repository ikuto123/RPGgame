using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class DialogueGraphEditor : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _fileName = "NewDialogue";
    private Toolbar _toolbar;
    private bool _hasUnsavedChanges = false;

    //画面分割用の要素
    private VisualElement _leftPanel; 
    private VisualElement _rightPanel; 
    [System.NonSerialized]
    private Editor _cachedEditor; 

    [MenuItem("Graph/Dialogue Editor")]
    public static void OpenGraphWindow()
    {
        var window = GetWindow<DialogueGraphEditor>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    public static void OpenGraphWithFile(string fileName)
    {
        var window = GetWindow<DialogueGraphEditor>();
        window.titleContent = new GUIContent("Dialogue Graph");
        window.Show();
        window.LoadFileFromHandler(fileName);
    }
    
    private void LoadFileFromHandler(string fileName)
    {
        _fileName = fileName;
        InitializeEditor(false); 
        GraphSaveUtility.LoadGraph(_fileName, _graphView);
        _hasUnsavedChanges = false; 
    }

    private void OnEnable()
    {
        ShowStartupScreen();
    }

    private void OnDisable()
    {
        if (_cachedEditor != null) DestroyImmediate(_cachedEditor);
        if (_graphView != null) _graphView.RemoveFromHierarchy();
    }

    private void ShowStartupScreen()
    {
        rootVisualElement.Clear();
        rootVisualElement.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        rootVisualElement.style.justifyContent = Justify.Center;
        rootVisualElement.style.alignItems = Align.Center;
        rootVisualElement.style.flexDirection = FlexDirection.Column;

        var title = new Label("Dialogue Editor");
        title.style.fontSize = 24;
        title.style.marginBottom = 20;
        title.style.color = Color.white;
        rootVisualElement.Add(title);

        var buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;

        var createButton = new Button(() => InitializeEditor(true)) { text = "Create New Graph" };
        createButton.style.width = 150;
        createButton.style.height = 40;
        createButton.style.marginRight = 10;
        buttonContainer.Add(createButton);

        var loadButton = new Button(() => 
        {
            LoadFileViaPanel();
        }) 
        { text = "Load Existing Graph" };
        loadButton.style.width = 150;
        loadButton.style.height = 40;
        buttonContainer.Add(loadButton);

        rootVisualElement.Add(buttonContainer);
    }

    // ファイル選択パネルを開く処理（共通化）
    private void LoadFileViaPanel()
    {
        string path = EditorUtility.OpenFilePanel("Load Dialogue Graph", "Assets/Resources", "asset");
        if (!string.IsNullOrEmpty(path))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            _fileName = fileName;
            InitializeEditor(false);
            GraphSaveUtility.LoadGraph(_fileName, _graphView);
            _hasUnsavedChanges = false; 
        }
    }

    private void InitializeEditor(bool isNew)
    {
        rootVisualElement.Clear();
        
        // レイアウトを横並びに
        rootVisualElement.style.flexDirection = FlexDirection.Row;
        rootVisualElement.style.alignItems = Align.Stretch;

        // 1. 左パネル（インスペクター）
        _leftPanel = new VisualElement();
        _leftPanel.style.width = 300; 
        _leftPanel.style.borderRightWidth = 2;
        _leftPanel.style.borderRightColor = new Color(0.1f, 0.1f, 0.1f);
        _leftPanel.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
        _leftPanel.style.paddingLeft = 10;
        _leftPanel.style.paddingRight = 10;
        _leftPanel.style.paddingTop = 10;
        _leftPanel.style.flexShrink = 0; 
        
        var inspectorLabel = new Label("Node Properties") { style = { fontSize = 18, marginBottom = 10, unityFontStyleAndWeight = FontStyle.Bold } };
        _leftPanel.Add(inspectorLabel);
        
        var inspectorContainer = new ScrollView(); 
        _leftPanel.Add(inspectorContainer);

        rootVisualElement.Add(_leftPanel);

        // 2. 右パネル（グラフ）
        _rightPanel = new VisualElement();
        _rightPanel.style.flexGrow = 1; 
        _rightPanel.style.flexDirection = FlexDirection.Column; // 縦並びにする

        GenerateToolbar(); 

        _graphView = new DialogueGraphView { name = "Dialogue Graph" };
        _graphView.style.flexGrow = 1;       
        
        _hasUnsavedChanges = false;
        _graphView.graphViewChanged = (changes) => { _hasUnsavedChanges = true; return changes; };
        _graphView.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt => _hasUnsavedChanges = true);
        _graphView.RegisterCallback<ChangeEvent<string>>(evt => _hasUnsavedChanges = true);

        // イベント登録
        _graphView.OnNodeSelected = OnNodeSelectionChanged;

        _rightPanel.Add(_graphView); 

        rootVisualElement.Add(_rightPanel);

        if (isNew)
        {
            var node = _graphView.CreateDialogueNode("Start Node");
            _graphView.AddElement(node);
            _fileName = "NewDialogue";
            RefreshFileNameField();
            _hasUnsavedChanges = false;
        }
    }

    private void OnNodeSelectionChanged(DialogueNode node)
    {
        var container = _leftPanel.Q<ScrollView>();
        if (container == null) return;
        
        container.Clear(); 
        if (_cachedEditor != null) DestroyImmediate(_cachedEditor);

        if (node == null)
        {
            container.Add(new Label("Select a node to edit."));
            return;
        }

        switch (node.Type)
        {
            case NodeType.Dialogue:
                DrawDialogueInspector(node, container);
                break;
            case NodeType.Branch:
                DrawBranchInspector(node, container);
                break;
            case NodeType.SetVariable:
                DrawSetVariableInspector(node, container);
                break;
        }
    }
    
    private void DrawDialogueInspector(DialogueNode node, VisualElement container)
    {
        container.Add(new Label("Dialogue Settings") { style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });

        if (node.EventData == null)
        {
            var nameField = new TextField("New File Name:");
            string defaultName = $"Dialogue_{node.title.Replace(" ", "_")}";
            nameField.value = defaultName;
            container.Add(nameField);
             
            var createButton = new Button(() => { CreateNewEventData(node, nameField.value); }) { text = "Create Data" };
            container.Add(createButton);
        }
        else
        {
            _cachedEditor = Editor.CreateEditor(node.EventData);
            var inspectorImgui = new IMGUIContainer(() => { if(_cachedEditor.target) _cachedEditor.OnInspectorGUI(); });
            container.Add(inspectorImgui);
        }
    }
    
    private void DrawBranchInspector(DialogueNode node, VisualElement container)
    {
        container.Add(new Label("Branch Logic (If Statement)") { style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });

        // 変数名の入力
        var varField = new TextField("Target Variable Name:");
        varField.value = node.TargetVariable;
        varField.RegisterValueChangedCallback(evt => {
            node.TargetVariable = evt.newValue;
            _hasUnsavedChanges = true; // 保存フラグを立てる重要性
        });
        container.Add(varField);

        container.Add(new Label("Output Logic:") { style = { marginTop = 10, unityFontStyleAndWeight = FontStyle.Bold } });
        container.Add(new Label("- Connect logic to Ports based on value."));
        container.Add(new Label("- Example Port Names: '1', '0', 'true', 'default'"));
    }
    
    private void DrawSetVariableInspector(DialogueNode node, VisualElement container)
    {
        container.Add(new Label("Variable Operation") { style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold } });

        // 変数名の入力
        var varField = new TextField("Variable Name:");
        varField.value = node.TargetVariable;
        varField.RegisterValueChangedCallback(evt => {
            node.TargetVariable = evt.newValue;
            _hasUnsavedChanges = true;
        });
        container.Add(varField);

        // 操作タイプ (Set, Add)
        var opField = new EnumField("Operation Type", node.OperationType);
        opField.RegisterValueChangedCallback(evt => {
            node.OperationType = (VariableOpType)evt.newValue;
            _hasUnsavedChanges = true;
        });
        container.Add(opField);

        // 値の入力
        var valField = new IntegerField("Value:");
        valField.value = node.TargetValue;
        valField.RegisterValueChangedCallback(evt => {
            node.TargetValue = evt.newValue;
            _hasUnsavedChanges = true;
        });
        container.Add(valField);
    }
    
    private void CreateNewEventData(DialogueNode node, string fileName)
    {
        string folderPath = "Assets/conversation/DialogueEvents";
        
        // フォルダ確認
        if (!AssetDatabase.IsValidFolder("Assets/conversation")) AssetDatabase.CreateFolder("Assets", "conversation");
        if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/conversation", "DialogueEvents");

        // インスタンス作成
        ConversationEventSO newEvent = ScriptableObject.CreateInstance<ConversationEventSO>();
        
        // パスを作成
        string fullPath = $"{folderPath}/{fileName}.asset";
        
        fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
        
        // アセット作成
        AssetDatabase.CreateAsset(newEvent, fullPath);
        AssetDatabase.SaveAssets();

        // ノードに割り当て
        node.EventData = newEvent;
        
        // インスペクターを再描画して、すぐ編集できるようにする
        OnNodeSelectionChanged(node);
        
        Debug.Log($"Created new event data: {fullPath}");
    }
    private void GenerateToolbar()
    {
        _toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name:");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => {
            _fileName = evt.newValue;
            _hasUnsavedChanges = true;
        });
        _toolbar.Add(fileNameTextField);

        // Save Data
        _toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });

        _toolbar.Add(new Button(() => LoadFileViaPanel()) { text = "Load Data" });

        // Create Node
        _toolbar.Add(new Button(() => 
        {
            _graphView.CreateDialogueNode("Dialogue Node"); 
        }) { text = "Add Dialogue" }); 

        // 2. 分岐ノード
        _toolbar.Add(new Button(() => 
        {
            _graphView.CreateBranchNode("Branch Logic");
        }) { text = "Add Branch" });

        // 3. 変数操作ノード
        _toolbar.Add(new Button(() => 
        {
            _graphView.CreateSetVariableNode("Set Variable");
        }) { text = "Add Set Var" });
        
        // Back to Menu
        _toolbar.Add(new Button(() => OnBackToMenu()) { text = "Back to Menu" });

        _rightPanel.Add(_toolbar);
    }
    
    private void RefreshFileNameField()
    {
        if (_toolbar != null)
        {
            var field = _toolbar.Q<TextField>();
            if (field != null) field.value = _fileName;
        }
    }

    private void OnBackToMenu()
    {
        if (_hasUnsavedChanges)
        {
            int option = EditorUtility.DisplayDialogComplex(
                "Unsaved Changes",
                "You have unsaved changes. Do you want to save before exiting?",
                "Save", "Cancel", "Don't Save"
            );
            switch (option)
            {
                case 0: RequestDataOperation(true); ShowStartupScreen(); break;
                case 1: return;
                case 2: ShowStartupScreen(); break;
            }
        }
        else ShowStartupScreen();
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name", "Please enter a valid file name.", "OK");
            return;
        }

        if (save)
        {
            string path = $"Assets/Resources/conversationNodes/{_fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueContainer>(path) != null)
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "File Already Exists",
                    $"'{_fileName}' already exists.\nDo you want to overwrite it or save as a new file?",
                    "Overwrite", "Save as New", "Cancel"
                );
                switch (option)
                {
                    case 0: GraphSaveUtility.SaveGraph(_fileName, _graphView, true); _hasUnsavedChanges = false; break;
                    case 1: GraphSaveUtility.SaveGraph(_fileName, _graphView, false); _hasUnsavedChanges = false; break;
                    case 2: break;
                }
            }
            else
            {
                GraphSaveUtility.SaveGraph(_fileName, _graphView, false);
                _hasUnsavedChanges = false;
            }
        }
    }
}