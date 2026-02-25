using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerDataUnit", menuName = "Unit/PlayerDataUnit")]
public class PlayerDataUnit : ScriptableObject
{
    // 割り当てられたカテゴリーID (アタッチ維持用)
    [HideInInspector] public string _targetCategoryId;

    [SerializeField] private List<PlayerData> dataList;

    private Dictionary<string, PlayerData> _idMap;
    [System.NonSerialized] private bool _isInitialized = false;

    private void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _idMap = new Dictionary<string, PlayerData>();
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

    public PlayerData Get(string id)
    {
        Initialize();
        if (_idMap != null && _idMap.TryGetValue(id, out PlayerData val)) return val;
        return null;
    }

    public List<PlayerData> GetAll() => dataList;
}
