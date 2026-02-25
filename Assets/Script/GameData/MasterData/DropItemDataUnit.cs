using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DropItemDataUnit", menuName = "Unit/DropItemDataUnit")]
public class DropItemDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<DropItemData> dataList;

    private Dictionary<string, DropItemData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, DropItemData>();
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

    public DropItemData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out DropItemData val)) return val;
        return null;
    }

    public List<DropItemData> GetAll() => dataList;
}
