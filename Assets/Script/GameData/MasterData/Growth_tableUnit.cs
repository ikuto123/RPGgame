using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Growth_tableUnit", menuName = "Unit/Growth_tableUnit")]
public class Growth_tableUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<Growth_table> dataList;

    private Dictionary<string, Growth_table> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, Growth_table>();
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

    public Growth_table Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out Growth_table val)) return val;
        return null;
    }

    public List<Growth_table> GetAll() => dataList;
}
