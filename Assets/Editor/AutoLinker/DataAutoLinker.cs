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
        LinkStatus<ItemStatus , ItemData>("itemData");
        LinkStatus<EquipmentStatus , EquipmentData>("equipmentData");
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
}