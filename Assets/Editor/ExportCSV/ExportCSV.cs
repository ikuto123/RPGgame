using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;

public partial class ExportCSV : EditorWindow
{
    [MenuItem("Tools/ExportCSV")]
    private static void ShowWindow()
    {
        var window = GetWindow<ExportCSV>("UIElements");
        window.titleContent = new GUIContent("ExportCSV");
        window.Show();
    }

    private DefaultAsset Obj = null;
    private string path;
    private string ext;
    
    // 🌟 出力先パスを保持する変数
    private string outputPath = "";

    private void OnEnable()
    {
        // ウィンドウを開いた時の初期値（プロジェクトのAssetsフォルダ）を設定
        outputPath = Application.dataPath;
    }

    private void OnGUI()
    {
        // --- 1. Excelファイルの選択 ---
        EditorGUILayout.LabelField("1. Select Excel File", EditorStyles.boldLabel);
        var newObj = (DefaultAsset)EditorGUILayout.ObjectField("Excel File", Obj, typeof(DefaultAsset), false);

        if (newObj != Obj)
        {
            if (newObj != null)
            {
                path = AssetDatabase.GetAssetPath(newObj);
                ext = Path.GetExtension(path).ToLower();

                if (ext == ".xlsx" || ext == ".xls")
                {
                    Obj = newObj;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Excelファイル（.xlsx / .xls）だけ選択できます！");
                    Obj = null;
                    path = "";
                }
            }
            else
            {
                Obj = null;
                path = "";
            }
        }

        if (Obj != null)
        {
            EditorGUILayout.LabelField("Selected Path:", path);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // --- 2. 出力先フォルダの選択 UIを追加 ---
        EditorGUILayout.LabelField("2. Select Output Folder", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            // フォルダ選択ダイアログを開く
            string defaultFolder = string.IsNullOrEmpty(outputPath) ? Application.dataPath : outputPath;
            string selectedFolder = EditorUtility.OpenFolderPanel("Select CSV Output Folder", defaultFolder, "");
            
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                outputPath = selectedFolder;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // --- 3. 実行ボタン ---
        if (GUILayout.Button("Export CSV", GUILayout.Height(30)))
        {
            if (Obj == null)
            {
                UnityEngine.Debug.LogWarning("Excelファイルを選択してください！");
                return;
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                UnityEngine.Debug.LogWarning("出力先のフォルダを指定してください！");
                return;
            }

            Pythonexecution(path, outputPath);
        }
    }

    // 引数に出力先パス (outPath) を追加
    private void Pythonexecution(string ExcelPath, string outPath)
    {
        string excelFullPath = Path.GetFullPath(ExcelPath);
        
        string pyExePath = @"C:\Python312\python.exe";
        // Pythonスクリプトの場所も相対パスから絶対パスに変換しておく（安定します）
        string pyCodePath = Path.GetFullPath(Path.Combine(Application.dataPath, @"Script\DataManage\Excel_ExportCSV.py")); 

        var psi = new ProcessStartInfo()
        {
            FileName = pyExePath,
            Arguments = $"\"{pyCodePath}\" \"{excelFullPath}\" \"{outPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8, // 文字化け対策
            StandardErrorEncoding = System.Text.Encoding.UTF8   // 文字化け対策
        };

        var process = new Process();
        process.StartInfo = psi;
        process.Start();

        // ログを取得
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        process.WaitForExit();

        // UnityコンソールにPythonの結果を表示
        if (!string.IsNullOrEmpty(stdout)) UnityEngine.Debug.Log($"[Python Output]\n{stdout}");
        if (!string.IsNullOrEmpty(stderr)) UnityEngine.Debug.LogError($"[Python Error]\n{stderr}");

        UnityEngine.Debug.Log($"Python ExitCode: {process.ExitCode}");
        
        if (process.ExitCode == 0) 
        { 
            UnityEngine.Debug.Log("<color=green>CSV Export Completed!</color>");
            AssetDatabase.Refresh(); 
        }
        
        process.Close();
    }
}