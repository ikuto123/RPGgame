using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Magic_Attribute_TableUnit", menuName = "Unit/Magic_Attribute_TableUnit")]
public class Magic_Attribute_TableUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<Magic_Attribute_Table> dataList;

    private Dictionary<string, Magic_Attribute_Table> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, Magic_Attribute_Table>();
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

    public Magic_Attribute_Table Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out Magic_Attribute_Table val)) return val;
        return null;
    }

    public List<Magic_Attribute_Table> GetAll() => dataList;
}
