using UnityEditor;
using UnityEngine;
using System;

public enum ItemImageCategory
{
    All,
    Item,
    Weapon,
    Armor,
    Accessory,
    Unknown
}

[CustomEditor(typeof(ItemImageData))]
public class ItemImageDataEditor : Editor
{
    private string _searchId = "";
    private ItemImageCategory _filterCategory = ItemImageCategory.All;

    public override void OnInspectorGUI()
    {
        ItemImageData data = (ItemImageData)target;

        if (data.ItemImageDataList == null)
        {
            EditorGUILayout.HelpBox("ItemImageDataList が null です。", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Item Image Data", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("検索・絞り込み", EditorStyles.boldLabel);

        _searchId = EditorGUILayout.TextField("ID検索", _searchId);
        _filterCategory = (ItemImageCategory)EditorGUILayout.EnumPopup("カテゴリ絞り込み", _filterCategory);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("ID昇順で並び替え"))
        {
            Undo.RecordObject(data, "Sort ItemImageData Asc");
            SortById(data, true);
            EditorUtility.SetDirty(data);
        }

        if (GUILayout.Button("ID降順で並び替え"))
        {
            Undo.RecordObject(data, "Sort ItemImageData Desc");
            SortById(data, false);
            EditorUtility.SetDirty(data);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("一覧", EditorStyles.boldLabel);

        int visibleCount = 0;

        for (int i = 0; i < data.ItemImageDataList.Count; i++)
        {
            var item = data.ItemImageDataList[i];
            if (item == null) continue;

            string id = item.ItemId ?? "";

            bool matchSearch = string.IsNullOrEmpty(_searchId) ||
                               id.IndexOf(_searchId, StringComparison.OrdinalIgnoreCase) >= 0;

            bool matchCategory = _filterCategory == ItemImageCategory.All ||
                                 GetCategoryFromId(id) == _filterCategory;

            if (!matchSearch || !matchCategory)
                continue;

            visibleCount++;

            EditorGUILayout.BeginVertical("box");

            EditorGUI.BeginChangeCheck();

            string newName = EditorGUILayout.TextField("Item Image Name", item.ItemImageName);
            Sprite newSprite = (Sprite)EditorGUILayout.ObjectField("Item Image", item.ItemImage, typeof(Sprite), false);
            string newId = EditorGUILayout.TextField("Item Id", item.ItemId);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(data, "Edit ItemImageData");
                SetPrivateField(item, "itemImageName", newName);
                SetPrivateField(item, "itemImage", newSprite);
                SetPrivateField(item, "itemId", newId);
                EditorUtility.SetDirty(data);
            }

            if (GUILayout.Button("削除"))
            {
                Undo.RecordObject(data, "Remove ItemImageData");
                data.ItemImageDataList.RemoveAt(i);
                EditorUtility.SetDirty(data);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        if (visibleCount == 0)
        {
            EditorGUILayout.HelpBox("該当するデータがありません。", MessageType.Info);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("要素を追加"))
        {
            Undo.RecordObject(data, "Add ItemImageData");
            data.ItemImageDataList.Add(new ItemImageData.Itemimage());
            EditorUtility.SetDirty(data);
        }
    }

    private void SortById(ItemImageData data, bool ascending)
    {
        var list = data.ItemImageDataList;
        if (list == null) return;

        if (ascending)
            list.Sort((a, b) => EditorUtility.NaturalCompare(a?.ItemId ?? "", b?.ItemId ?? ""));
        else
            list.Sort((a, b) => EditorUtility.NaturalCompare(b?.ItemId ?? "", a?.ItemId ?? ""));
    }

    private ItemImageCategory GetCategoryFromId(string id)
    {
        if (string.IsNullOrEmpty(id)) return ItemImageCategory.Unknown;

        id = id.ToLower();

        if (id.StartsWith("ite")) return ItemImageCategory.Item;
        if (id.StartsWith("wep")) return ItemImageCategory.Weapon;
        if (id.StartsWith("arm")) return ItemImageCategory.Armor;
        if (id.StartsWith("acc")) return ItemImageCategory.Accessory;

        return ItemImageCategory.Unknown;
    }

    private void SetPrivateField(object targetObj, string fieldName, object value)
    {
        var field = targetObj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(targetObj, value);
        }
    }
}