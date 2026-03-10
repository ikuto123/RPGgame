using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DataAutoLinker
{
    public void LinkData()
    {
        LinkStatus<PlayerStatus, PlayerData>("playerData");
        LinkStatus<EnemyStatus, EnemyData>("enemyData");
        LinkStatus<ItemStatus, ItemData>("itemData");
        LinkStatus<EquipmentStatus, EquipmentData>("equipmentData");

        // 引数: <コンポーネント, データ型>(IDのリスト変数名, データのリスト変数名)
        //LinkStatusList<ShopEquipment, EquipmentData>("equipmentIdList", "equipmentList", "equipmentData");
        
    }

    private static void LinkStatus<TStatus, TData>(string dataFieldName)
        where TStatus : MonoBehaviour
        where TData : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(TData).Name}");
        List<TData> allDataList = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<TData>(AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        // シーン内の対象コンポーネントを検索
        TStatus[] targets = Object.FindObjectsByType<TStatus>(FindObjectsSortMode.None);

        int count = 0;

        foreach (TStatus target in targets)
        {
            //キャラクターのスクリプトからIDとデータフィールドを取得
            SerializedObject so = new SerializedObject(target);
            SerializedProperty idProp = so.FindProperty("Id");
            SerializedProperty dataProp = so.FindProperty(dataFieldName);

            if (idProp == null)
            {
                Debug.LogWarning($"{target.name} に 'Id' フィールドが見つかりません。{typeof(TStatus).Name} の変数名が 'Id' か確認してください。");
                continue;
            }

            if (dataProp == null)
            {
                Debug.LogWarning($"{target.name} に '{dataFieldName}' フィールドが見つかりません。");
                continue;
            }

            string targetID = idProp.stringValue;

            //スクリプタブルオブジェクトからIDが一致するデータを探す
            TData matchedData = allDataList.FirstOrDefault(d =>
            {
                var prop = d.GetType().GetProperty("Id");
                if (prop != null)
                {
                    //文字列として比較
                    var val = prop.GetValue(d) as string;
                    return val == targetID;
                }

                return false;
            });

            if (matchedData != null)
            {
                if (dataProp.objectReferenceValue != matchedData)
                {
                    dataProp.objectReferenceValue = matchedData;
                    so.ApplyModifiedProperties();
                    count++;
                }
            }
        }

        if (count > 0)
        {
            Debug.Log($"{typeof(TStatus).Name}: {count} 体のオブジェクトにデータをアタッチしました。");
        }
        else
        {
            Debug.Log($"{typeof(TStatus).Name}: 更新対象が見つかりませんでした。(ID一致なし、または更新不要)");
        }
    }

    private static void LinkStatusList<TStatus, TData>(string idListName, string dataListName,
        string nestedDataFieldName = "")
        where TStatus : MonoBehaviour
        where TData : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(TData).Name}");
        List<TData> allDataList = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<TData>(AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        TStatus[] targets = Object.FindObjectsByType<TStatus>(FindObjectsSortMode.None);
        int count = 0;

        foreach (TStatus target in targets)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty idListProp = so.FindProperty(idListName); // IDのリスト
            SerializedProperty dataListProp = so.FindProperty(dataListName); // データのリスト

            if (idListProp == null)
            {
                Debug.LogWarning($"{target.name} に '{idListName}' が見つかりません。");
                continue;
            }

            if (dataListProp == null)
            {
                Debug.LogWarning($"{target.name} に '{dataListName}' が見つかりません。");
                continue;
            }

            // データリストの枠の数を、IDリストの数に合わせる
            dataListProp.arraySize = idListProp.arraySize;

            for (int i = 0; i < idListProp.arraySize; i++)
            {
                string targetID = idListProp.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(targetID)) continue;

                // IDが一致するデータを探す
                TData matchedData = allDataList.FirstOrDefault(d =>
                {
                    var type = d.GetType();
                    var prop = type.GetProperty("Id");
                    if (prop != null && (prop.GetValue(d) as string) == targetID) return true;

                    var field = type.GetField("Id");
                    if (field != null && (field.GetValue(d) as string) == targetID) return true;

                    return false;
                });

                if (matchedData != null)
                {
                    // リストの i 番目の要素を取得
                    SerializedProperty elementProp = dataListProp.GetArrayElementAtIndex(i);

                    // 第3引数が指定されていれば、そのクラスの中の変数を探す。指定がなければ直接代入する。
                    SerializedProperty targetProp = string.IsNullOrEmpty(nestedDataFieldName)
                        ? elementProp
                        : elementProp.FindPropertyRelative(nestedDataFieldName);

                    if (targetProp != null)
                    {
                        targetProp.objectReferenceValue = matchedData;
                        count++;
                    }
                    else
                    {
                        Debug.LogWarning($"{target.name}: リスト要素内に '{nestedDataFieldName}' という変数が見つかりません。");
                    }
                }
                else
                {
                    Debug.LogWarning($"{typeof(TStatus).Name}: ID '{targetID}' に一致する {typeof(TData).Name} が見つかりません。");
                }
            }

            so.ApplyModifiedProperties();
        }

        if (count > 0)
        {
            Debug.Log($"{typeof(TStatus).Name}: {count} 件のリストデータをリンクしました。");
        }
    }
}