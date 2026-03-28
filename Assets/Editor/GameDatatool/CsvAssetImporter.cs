using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class CsvAssetImporter
{
    public static void ImportAll(DefaultAsset csvFolder, string baseSavePath)
    {
        string folderPath = AssetDatabase.GetAssetPath(csvFolder);
        string[] files = Directory.GetFiles(folderPath, "*.csv");

        if (files.Length == 0)
        {
            Debug.LogWarning("CSVファイルが見つかりませんでした。");
            return;
        }

        // ★安定化対策: 処理中のUnityの自動更新を止めて一気に行う
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                EditorUtility.DisplayProgressBar("CSV Import", $"Updating {Path.GetFileName(file)}...", (float)i / files.Length);
                ImportData(file, baseSavePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Error] インポート中にエラーが発生しました: {e.Message}");
        }
        finally
        {
            // 更新再開
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Complete", "データの更新が完了しました。\n(参照は維持されています)", "OK");
    }

    private static void ImportData(string filePath, string baseSavePath)
    {
        string className = Path.GetFileNameWithoutExtension(filePath);

        System.Type classType = System.Type.GetType(className + ", Assembly-CSharp");
        if (classType == null)
        {
            Debug.LogError($"[Skip] クラス '{className}' が見つかりません。先にスクリプト生成を行ってください。");
            return;
        }

        if (!File.Exists(filePath)) return;
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        if (lines.Length < 4) return; 

        // ヘッダー処理
        string headerLine = lines[1];
        if (headerLine.Length > 0 && headerLine[0] == '\ufeff') headerLine = headerLine.Substring(1);
        string[] varNames = SplitCsvLine(headerLine);

        // フォルダ準備 (削除はせず、なければ作るだけ)
        string targetFolder = Path.Combine(baseSavePath, className).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            CreateFolderRecursive(targetFolder);
        }

        // データ行処理
        for (int i = 3; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] rowData = SplitCsvLine(line);

            string idStr = (rowData.Length > 0) ? rowData[0].Trim() : i.ToString();
            if (string.IsNullOrEmpty(idStr)) idStr = i.ToString();

            string assetName = $"{className}_{idStr}";
            string assetPath = Path.Combine(targetFolder, assetName + ".asset").Replace("\\", "/");

            // =========================================================
            // 【修正点】ロードして存在確認 -> なければ作成
            // =========================================================
            ScriptableObject so = AssetDatabase.LoadAssetAtPath(assetPath, classType) as ScriptableObject;

            if (so == null)
            {
                // 新規作成
                so = ScriptableObject.CreateInstance(classType);
                so.name = assetName;
                AssetDatabase.CreateAsset(so, assetPath);
            }
            // 既に存在する場合は、その so をそのまま使って値を上書きする
            // =========================================================

            SerializedObject serializedSo = new SerializedObject(so);
            serializedSo.Update(); // 現在の値を読み込む

            for (int col = 0; col < varNames.Length; col++)
            {
                if (col >= rowData.Length) break;

                string rawVarName = varNames[col].Trim();
                if (string.IsNullOrEmpty(rawVarName)) continue;

                string varName = Regex.Replace(rawVarName, @"[^\w]", "_");
                if (!string.IsNullOrEmpty(varName) && char.IsDigit(varName[0])) varName = "_" + varName;

                string valueStr = rowData[col];
                if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                {
                    valueStr = valueStr.Substring(1, valueStr.Length - 2).Replace("\"\"", "\"");
                }
                valueStr = valueStr.Trim();

                SerializedProperty prop = serializedSo.FindProperty(varName);
                if (prop == null && !string.IsNullOrEmpty(varName))
                {
                    string lowerFirst = char.ToLower(varName[0]) + varName.Substring(1);
                    prop = serializedSo.FindProperty(lowerFirst);
                }

                if (prop != null)
                {
                    SetPropertyValue(prop, valueStr);
                }
            }

            serializedSo.ApplyModifiedProperties(); // 変更を適用
            EditorUtility.SetDirty(so); // 保存対象としてマーク
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    private static void SetPropertyValue(SerializedProperty prop, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        try
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (int.TryParse(value, out int intVal)) prop.intValue = intVal;
                    break;
                case SerializedPropertyType.Float:
                    if (float.TryParse(value, out float floatVal)) prop.floatValue = floatVal;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = value;
                    break;
                case SerializedPropertyType.Boolean:
                    if (bool.TryParse(value, out bool boolVal)) prop.boolValue = boolVal;
                    break;
            }
        }
        catch { }
    }
    
    private static void CreateFolderRecursive(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(parent)) CreateFolderRecursive(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}