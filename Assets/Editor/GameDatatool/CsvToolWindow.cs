using UnityEditor;
using UnityEngine;

public class CsvToolWindow : EditorWindow
{
    private DefaultAsset csvFolder;
    
    private string scriptSavePath = "Assets/Script/GameData";
    private string assetSavePath = "Assets/Resources/GameData";
    private string masterDataSavePath = "Assets/Resources/MasterData";
    
    private string resourcesRootPath = "GameData"; 

    private string categoryVarName = "categoryId";
    private string commonIdVarName = "Id"; 

    private bool groupByCategoryFolder = false;

    [MenuItem("Tools/Game Data Tool")]
    public static void ShowWindow()
    {
        GetWindow<CsvToolWindow>("Data Tool");
    }

    private void OnGUI()
    {
        // 1. CSV Tools
        GUILayout.Label("1. CSV Tools", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            csvFolder = (DefaultAsset)EditorGUILayout.ObjectField("CSV Folder", csvFolder, typeof(DefaultAsset), false);
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Scripts from CSV"))
            {
                if (csvFolder != null) CsvScriptGenerator.GenerateAll(csvFolder, scriptSavePath);
            }
            if (GUILayout.Button("Import Data from CSV"))
            {
                if (csvFolder != null) CsvAssetImporter.ImportAll(csvFolder, assetSavePath);
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        // Settings
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        scriptSavePath = EditorGUILayout.TextField("Script Save Path", scriptSavePath);
        assetSavePath = EditorGUILayout.TextField("Asset Save Path (Data)", assetSavePath);
        masterDataSavePath = EditorGUILayout.TextField("Master Save Path", masterDataSavePath);
        
        GUILayout.Space(10);
        
        // 2. Master Unit Generator
        GUILayout.Label("2. Master Unit Generator (All)", EditorStyles.boldLabel);
        GUILayout.Label("GameData内の全フォルダを一括処理します。", EditorStyles.helpBox);

        resourcesRootPath = EditorGUILayout.TextField("Resources Root Path", resourcesRootPath);
        commonIdVarName = EditorGUILayout.TextField("Common ID Var Name", commonIdVarName);
        categoryVarName = EditorGUILayout.TextField("Category Var Name", categoryVarName);

        GUILayout.Space(5);
        // ★フォルダ構成の選択
        groupByCategoryFolder = EditorGUILayout.ToggleLeft("Create Folder per Category (カテゴリーごとにフォルダ作成)", groupByCategoryFolder);

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        // 手順A: 全スクリプト生成
        if (GUILayout.Button("A. Gen All Scripts"))
        {
            // Scriptの中身でカテゴリーIDの型を string に統一しました
            CategoryUnitTool.GenerateAllUnitScripts(resourcesRootPath, scriptSavePath, commonIdVarName);
        }

        // 手順B: 全アセット生成
        if (GUILayout.Button("B. Create All Assets"))
        {
            CategoryUnitTool.CreateAllUnitAssets(resourcesRootPath, categoryVarName, masterDataSavePath, groupByCategoryFolder);
        }

        EditorGUILayout.EndHorizontal();
    }
}