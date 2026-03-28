using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDataUnit", menuName = "Unit/EnemyDataUnit")]
public class EnemyDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<EnemyData> dataList;

    private Dictionary<string, EnemyData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, EnemyData>();
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

    public EnemyData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out EnemyData val)) return val;
        return null;
    }

    public List<EnemyData> GetAll() => dataList;
}
