using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDataUnit", menuName = "Unit/ItemDataUnit")]
public class ItemDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<ItemData> dataList;

    private Dictionary<string, ItemData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, ItemData>();
        if (dataList == null) return;

        foreach (var item in dataList)
        {
            if (item == null) continue;
            try {
                string id = item.Id.ToString();
                if (!_idMap.ContainsKey(id)) _idMap.Add(id, item);
            } catch { }
        }
    }

    public ItemData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out ItemData val)) return val;
        return null;
    }

    public List<ItemData> GetAll() => dataList;
}
