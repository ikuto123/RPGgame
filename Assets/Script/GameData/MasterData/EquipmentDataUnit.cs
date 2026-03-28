using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EquipmentDataUnit", menuName = "Unit/EquipmentDataUnit")]
public class EquipmentDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<EquipmentData> dataList;

    private Dictionary<string, EquipmentData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, EquipmentData>();
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

    public EquipmentData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out EquipmentData val)) return val;
        return null;
    }

    public List<EquipmentData> GetAll() => dataList;
}
