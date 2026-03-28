#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

// エディタ起動時に自動でシステムを登録する
[InitializeOnLoad]
public static class GlobalReferenceChecker
{

    // =========================================================
    // ここに「あえて空にしている」例外リストを登録してください。
    // 書き方: { "スクリプト名", new string[] { "変数名1", "変数名2" } }
    // =========================================================
    private static readonly Dictionary<string, string[]> ignoreList = new Dictionary<string, string[]>()
    {
        { "Enemy", new string[] { "player", "targetNode" } },
        { "EquipmentShopView", new string[] { "optionalImage" } } // 必要に応じて書き換えてください
    };
    // =========================================================

    // スクリプトが読み込まれた時に、Playボタンの監視を開始する
    static GlobalReferenceChecker()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // Playボタンの状態が変わった瞬間に呼ばれる
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 「エディットモードから抜けようとした瞬間（Playモードに入る直前）」を検知
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (CheckAllCustomScripts())
            {
                // エラーがあったら、Playモードへの移行を完全にキャンセルする（一瞬も実行させない）
                EditorApplication.isPlaying = false;
                
                // コンソールウィンドウを自動的に開いて手前に持ってくる
                var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
                if (consoleWindowType != null)
                {
                    EditorWindow.GetWindow(consoleWindowType).Focus();
                }
            }
        }
    }

    // エラーがあったら true を返す処理
    private static bool CheckAllCustomScripts()
    {
        bool hasError = false;
        Object firstErrorScript = null; // 最初に見つけたエラースクリプトを記憶

        // シーン内のすべてのコンポーネントを取得
        MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var script in allScripts)
        {
            if (script == null) continue;

            System.Type type = script.GetType();
            string scriptName = type.Name;

            // 自分の制作したスクリプト（Assembly-CSharp）のみに絞る
            if (type.Assembly.GetName().Name != "Assembly-CSharp") continue;

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                bool isPublic = field.IsPublic;
                bool hasSerializeField = System.Attribute.IsDefined(field, typeof(SerializeField));

                if (!isPublic && !hasSerializeField) continue;

                if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    // 例外リストの確認
                    if (ignoreList.ContainsKey(scriptName))
                    {
                        if (System.Array.Exists(ignoreList[scriptName], name => name == field.Name))
                        {
                            continue; 
                        }
                    }

                    var value = field.GetValue(script);
                    bool isNull = (value == null) || (value is Object unityObj && unityObj == null);

                    if (isNull)
                    {
                        MonoScript monoScript = MonoScript.FromMonoBehaviour(script);
                        string assetPath = AssetDatabase.GetAssetPath(monoScript);

                        // 最初に見つけたエラースクリプトを記憶
                        if (firstErrorScript == null) firstErrorScript = monoScript;

                        // コンソールのリンクも10行目指定にしておく
                        Debug.LogError($"【アタッチ忘れ検知】{script.gameObject.name}  にある変数 '{field.Name}' が空です！", script.gameObject);
                        hasError = true;
                    }
                }
            }
        }

        if (hasError)
        {
            Debug.LogError("アタッチ忘れが見つかったため、実行を強制停止しました。");
            
            // ダイアログを表示
            bool openScript = EditorUtility.DisplayDialog(
                "アタッチ忘れの間抜けさんへ😢", 
                "アタッチされていない変数があります。\n残念ながら実行できません\n意図的にアタッチをしていない場合は「GlobalReferenceChecker」のスクリプトから例外処理を追加してください", 
                "わかりました"
            );

            // ダイアログでボタンが押されたら10行目を開く
            if (openScript && firstErrorScript != null)
            {
                AssetDatabase.OpenAsset(firstErrorScript, 10);
            }
        }
        else
        {
            Debug.Log("アタッチ忘れチェック完了：すべて正常にアタッチされています！");
        }

        return hasError;
    }
}
#endif