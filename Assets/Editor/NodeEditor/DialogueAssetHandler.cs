using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class DialogueAssetHandler
{
    // アセットが開かれたとき（ダブルクリック時）に呼ばれる処理
    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        // 選択されたオブジェクトを取得
        var obj = EditorUtility.InstanceIDToObject(instanceID);

        // それが会話データ（DialogueContainer）かチェック
        if (obj is DialogueContainer)
        {
            // パスを取得 (Assets/Resources/FileName.asset)
            string path = AssetDatabase.GetAssetPath(instanceID);
            
            // ファイル名だけを取り出す (例: FileName)
            // ※このシステムはResourcesロード前提でファイル名だけで管理しているため
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            // エディタウィンドウを開いてロードさせる
            DialogueGraphEditor.OpenGraphWithFile(fileName);
            
            return true; // 処理完了（Unity標準のInspector表示をキャンセル）
        }

        return false; // 違うファイルなら何もしない（通常通りInspectorが開く）
    }
}