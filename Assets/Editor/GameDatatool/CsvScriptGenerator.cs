using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class CsvScriptGenerator
{
    public static void GenerateAll(DefaultAsset csvFolder, string savePath)
    {
        string folderPath = AssetDatabase.GetAssetPath(csvFolder);
        if (!Directory.Exists(folderPath)) return;
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] files = Directory.GetFiles(folderPath, "*.csv");

        foreach (var file in files)
        {
            GenerateClassScript(file, savePath);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"{files.Length}個のスクリプト生成完了。\nコンパイル待ち後にデータ注入を行ってください。", "OK");
    }

    private static void GenerateClassScript(string filePath, string savePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        
        if (lines.Length < 3) return; // 行数不足チェック
        
        string[] englishHeaders = lines[1].Split(','); // 変数名
        string[] japaneseHeaders = lines[2].Split(','); // コメント
        
        List<string> fieldNames = new List<string>();
        List<string> fieldTypes = new List<string>();
        List<string> comments = new List<string>(); 

        int colCount = englishHeaders.Length;

        for (int col = 0; col < colCount; col++)
        {
            string varName = englishHeaders[col].Trim();
            if (string.IsNullOrEmpty(varName)) continue;

            // コメント取得
            string comment = (col < japaneseHeaders.Length) ? japaneseHeaders[col].Trim() : "";

            // サニタイズ
            varName = Regex.Replace(varName, @"[^\w]", "_");
            if (char.IsDigit(varName[0])) varName = "_" + varName;

            fieldNames.Add(varName);
            comments.Add(comment);

            // 型推論 (4行目以降)
            string detectedType = "int";
            bool isFloat = false;
            bool isString = false;

            for (int i = 3; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(',');
                if (col >= row.Length) continue;
                string val = row[col].Trim();
                if (string.IsNullOrEmpty(val)) continue;

                if (!int.TryParse(val, out _))
                {
                    if (float.TryParse(val, out _)) isFloat = true;
                    else { isString = true; break; }
                }
            }

            if (isString) detectedType = "string";
            else if (isFloat) detectedType = "float";

            fieldTypes.Add(detectedType);
        }
        
        //スクリプト書き出し(元のフォーマットを再現)
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System.Collections.Generic;"); // List等が使えるように念のため
        sb.AppendLine("");
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{fileName}\", menuName = \"ScriptableObjects/{fileName}\")]");
        sb.AppendLine($"public class {fileName} : ScriptableObject");
        sb.AppendLine("{");

        // private フィールド定義
        for (int i = 0; i < fieldNames.Count; i++)
        {
            sb.AppendLine($"    [SerializeField] private {fieldTypes[i]} {fieldNames[i]}; // {comments[i]}");
        }
        sb.AppendLine("");
        
        // public プロパティ定義
        for (int i = 0; i < fieldNames.Count; i++)
        {
            string fName = fieldNames[i];
            // 先頭大文字化
            string propName = char.ToUpper(fName[0]) + fName.Substring(1);
            sb.AppendLine($"    public {fieldTypes[i]} {propName} => {fName};");
        }

        sb.AppendLine("}");

        string fullPath = Path.Combine(savePath, fileName + ".cs");
        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
    }
}