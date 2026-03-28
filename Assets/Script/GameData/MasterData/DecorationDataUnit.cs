using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DecorationDataUnit", menuName = "Unit/DecorationDataUnit")]
public class DecorationDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<DecorationData> dataList;

    private Dictionary<string, DecorationData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, DecorationData>();
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

    public DecorationData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out DecorationData val)) return val;
        return null;
    }

    public List<DecorationData> GetAll() => dataList;
}
