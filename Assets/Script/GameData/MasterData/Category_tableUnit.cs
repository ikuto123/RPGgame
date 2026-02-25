using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Category_tableUnit", menuName = "Unit/Category_tableUnit")]
public class Category_tableUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<Category_table> dataList;

    private Dictionary<string, Category_table> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, Category_table>();
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

    public Category_table Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out Category_table val)) return val;
        return null;
    }

    public List<Category_table> GetAll() => dataList;
}
