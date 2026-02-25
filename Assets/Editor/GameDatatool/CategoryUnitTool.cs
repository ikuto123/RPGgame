using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

public static class CategoryUnitTool
{
    // =========================================================
    // バッチ処理 1: すべてのクラスのUnitスクリプトを一括生成 (String版)
    // =========================================================
    public static void GenerateAllUnitScripts(string resourcesRootPath, string savePath, string commonIdName)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", resourcesRootPath);
        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"フォルダが見つかりません: {fullPath}");
            return;
        }

        string[] directories = Directory.GetDirectories(fullPath);
        int count = 0;

        foreach (var dir in directories)
        {
            string className = Path.GetFileName(dir);
            System.Type type = System.Type.GetType(className + ", Assembly-CSharp");
            if (type == null) continue;

            GenerateUnitScript(className, savePath, commonIdName, showDialog: false);
            count++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"{count} 個のクラス用スクリプトを生成しました。\n(ID型: String固定)\n\nコンパイル完了後に「B. Create All Assets」を行ってください。", "OK");
    }

    // =========================================================
    // バッチ処理 2: すべてのデータのアセットを一括生成
    // =========================================================
    public static void CreateAllUnitAssets(string resourcesRootPath, string categoryVarName, string masterSavePath, bool groupByCategoryFolder)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", resourcesRootPath);
        if (!Directory.Exists(fullPath)) return;

        string[] directories = Directory.GetDirectories(fullPath);
        int totalCreated = 0;

        foreach (var dir in directories)
        {
            string className = Path.GetFileName(dir);
            string resPath = Path.Combine(resourcesRootPath, className).Replace("\\", "/");

            // 個別の生成処理を呼び出す
            // baseSavePath として masterSavePath をそのまま渡す
            totalCreated += CreateCategoryAssets(className, categoryVarName, resPath, masterSavePath, groupByCategoryFolder, showDialog: false);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Batch Complete", $"全データの処理が完了しました！\n総生成・更新数: {totalCreated}\n保存先: {masterSavePath}", "OK");
    }


    // =========================================================
    // 単体スクリプト生成 (String固定)
    // =========================================================
    public static void GenerateUnitScript(string targetClassName, string savePath, string idVarName, bool showDialog = true)
    {
        string unitClassName = targetClassName + "Unit";
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("");
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{unitClassName}\", menuName = \"Unit/{unitClassName}\")]");
        sb.AppendLine($"public class {unitClassName} : ScriptableObject");
        sb.AppendLine("{");
        sb.AppendLine("    // 割り当てられたカテゴリーID (アタッチ維持用)");
        sb.AppendLine("    [HideInInspector] public string _targetCategoryId;");
        sb.AppendLine("");
        sb.AppendLine($"    [SerializeField] private List<{targetClassName}> dataList;");
        sb.AppendLine("");
        sb.AppendLine($"    private Dictionary<string, {targetClassName}> _idMap;");
        sb.AppendLine("    [System.NonSerialized] private bool _isInitialized = false;");
        sb.AppendLine("");
        sb.AppendLine("    private void Initialize()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_isInitialized) return;");
        sb.AppendLine("        _isInitialized = true;");
        sb.AppendLine($"        _idMap = new Dictionary<string, {targetClassName}>();");
        sb.AppendLine("        if (dataList == null) return;");
        sb.AppendLine("");
        sb.AppendLine($"        foreach (var item in dataList)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (item == null) continue;");
        sb.AppendLine("            try {");
        sb.AppendLine($"                string id = item.{idVarName}.ToString();");
        sb.AppendLine("                if (!_idMap.ContainsKey(id)) _idMap.Add(id, item);");
        sb.AppendLine("            } catch { }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine($"    public {targetClassName} Get(string id)");
        sb.AppendLine("    {");
        sb.AppendLine("        Initialize();");
        sb.AppendLine($"        if (_idMap != null && _idMap.TryGetValue(id, out {targetClassName} val)) return val;");
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine($"    public List<{targetClassName}> GetAll() => dataList;");
        sb.AppendLine("}");

        savePath = savePath + "/MasterData";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        string fullPath = Path.Combine(savePath, unitClassName + ".cs");
        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);

        if (showDialog)
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", "スクリプトを生成しました(String版)。", "OK");
        }
    }

    // =========================================================
    // 単体アセット生成 (フォルダ構成切り替え対応)
    // =========================================================
    public static int CreateCategoryAssets(string targetClassName, string categoryVarName, string resourcesPath, string baseSavePath, bool groupByCategoryFolder, bool showDialog = true)
    {
        string unitClassName = targetClassName + "Unit";
        System.Type unitType = System.Type.GetType(unitClassName + ", Assembly-CSharp");
        System.Type itemType = System.Type.GetType(targetClassName + ", Assembly-CSharp");

        if (unitType == null) { Debug.LogError($"クラス {unitClassName} が見つかりません。"); return 0; }

        Object[] allData = Resources.LoadAll(resourcesPath, itemType);

        // カテゴリーID(文字列)ごとにデータをグルーピング
        Dictionary<string, List<Object>> groupedData = new Dictionary<string, List<Object>>();

        foreach (var obj in allData)
        {
            PropertyInfo prop = itemType.GetProperty(categoryVarName);
            FieldInfo field = itemType.GetField(categoryVarName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            string catId = "";
            bool found = false;

            // 文字列として取得
            if (prop != null) { var v = prop.GetValue(obj); catId = v?.ToString(); found = true; }
            else if (field != null) { var v = field.GetValue(obj); catId = v?.ToString(); found = true; }

            if (found && !string.IsNullOrEmpty(catId))
            {
                if (!groupedData.ContainsKey(catId)) groupedData[catId] = new List<Object>();
                groupedData[catId].Add(obj);
            }
        }

        int count = 0;
        foreach (var kvp in groupedData)
        {
            string catId = kvp.Key;
            List<Object> items = kvp.Value.OrderBy(x => x.name, new NaturalStringComparer()).ToList();

            // ファイル名に使えるようにサニタイズ
            string safeCatId = Regex.Replace(catId, @"[\\/:*?""<>|]", "_");

            string finalFolder;
            string assetName;

            if (groupByCategoryFolder)
            {
                // パターンA: カテゴリーフォルダの中に各クラスを入れる
                // 例: MasterData/Category_Sword/EquipmentData.asset
                //     MasterData/Category_Sword/ItemData.asset
                finalFolder = Path.Combine(baseSavePath, safeCatId).Replace("\\", "/");
                assetName = targetClassName; // ファイル名はクラス名にする
            }
            else
            {
                // パターンB: クラスフォルダの中にカテゴリーファイルを入れる (従来)
                // 例: MasterData/EquipmentData_Units/EquipmentData_Sword.asset
                finalFolder = Path.Combine(baseSavePath, targetClassName + "_Units").Replace("\\", "/");
                assetName = $"{targetClassName}_{safeCatId}";
            }

            if (!Directory.Exists(finalFolder)) Directory.CreateDirectory(finalFolder);
            string assetPath = Path.Combine(finalFolder, assetName + ".asset").Replace("\\", "/");

            // アセットの生成・ロード（既存があれば更新。見つかったら必要に応じて移動＝再アタッチ不要）
            ScriptableObject unitSO = AssetDatabase.LoadAssetAtPath(assetPath, unitType) as ScriptableObject;
            if (unitSO == null)
            {
                // まずは baseSavePath 配下で探す（速い）
                string foundPath;
                unitSO = FindExistingUnitAssetByCategoryId(unitType, catId, new[] { NormalizeAssetPath(baseSavePath) }, out foundPath);

                // 見つからなければプロジェクト全体（Assets）を検索
                if (unitSO == null)
                {
                    unitSO = FindExistingUnitAssetByCategoryId(unitType, catId, new[] { "Assets" }, out foundPath);
                }

                // 見つかった：必要なら移動（GUID維持 → 参照が切れない）
                if (unitSO != null)
                {
                    string fileName = Path.GetFileName(foundPath); // ← 既存の名前を維持
                    string desiredPath = Path.Combine(finalFolder, fileName).Replace("\\", "/");

                    if (foundPath != desiredPath)
                    {
                        string moveErr = AssetDatabase.MoveAsset(foundPath, desiredPath);
                        if (string.IsNullOrEmpty(moveErr)) assetPath = desiredPath;
                        else assetPath = foundPath; // 失敗したら現状維持
                    }
                    else
                    {
                        assetPath = foundPath;
                    }
                }
                else
                {
                    // 見つからない：新規作成
                    unitSO = ScriptableObject.CreateInstance(unitType);
                    AssetDatabase.CreateAsset(unitSO, assetPath);
                }
            }

            SerializedObject so = new SerializedObject(unitSO);
            so.Update();

            // ID記録 (文字列に変更)
            SerializedProperty propId = so.FindProperty("_targetCategoryId");
            if (propId != null) propId.stringValue = catId;

            // リスト更新
            SerializedProperty propList = so.FindProperty("dataList");
            if (propList != null)
            {
                propList.ClearArray();
                for (int i = 0; i < items.Count; i++)
                {
                    propList.InsertArrayElementAtIndex(i);
                    propList.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(unitSO);
            count++;
        }

        if (showDialog)
        {
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Complete", $"処理完了: {count}件\n保存先: {baseSavePath}", "OK");
        }
        return count;
    }


    // =========================================================
    // 既存Unitアセット探索（_targetCategoryId が一致するものを探す）
    // 見つかった場合は GUID を維持したまま更新できるため、参照（アタッチ）をやり直さなくてOK
    // =========================================================
    private static ScriptableObject FindExistingUnitAssetByCategoryId(System.Type unitType, string categoryId, string[] searchFolders, out string foundPath)
    {
        foundPath = null;

        if (unitType == null || string.IsNullOrEmpty(categoryId)) return null;

        // ※ Unity の FindAssets はプロパティ値では絞れないので、型で検索してから中身をチェックする
        string filter = $"t:{unitType.Name}";
        string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath(path, unitType) as ScriptableObject;
            if (asset == null) continue;

            var so = new SerializedObject(asset);
            var prop = so.FindProperty("_targetCategoryId");
            if (prop != null && prop.propertyType == SerializedPropertyType.String && prop.stringValue == categoryId)
            {
                foundPath = path;
                return asset;
            }
        }

        return null;
    }

    private static string NormalizeAssetPath(string p)
    {
        return string.IsNullOrEmpty(p) ? p : p.Replace("\\", "/").TrimEnd('/');
    }

    public class NaturalStringComparer : IComparer<string>
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);
        public int Compare(string x, string y) => StrCmpLogicalW(x, y);
    }
}
