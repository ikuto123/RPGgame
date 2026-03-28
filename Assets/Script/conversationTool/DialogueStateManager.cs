using System.Collections.Generic;
using UnityEngine;

public class DialogueStateManager : MonoBehaviour
{
    public static DialogueStateManager Instance { get; private set; }
    private Dictionary<string, int> _variables = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetInt(string key, int value)
    {
        _variables[key] = value;
        Debug.Log($"<color=cyan>[StateManager: Set]</color> 変数 '{key}' を {value} に設定しました。");
    }

    public void AddInt(string key, int value)
    {
        if (_variables.ContainsKey(key))
        {
            _variables[key] += value;
        }
        else
        {
            _variables[key] = value;
        }
        Debug.Log($"<color=green>[StateManager: Add]</color> 変数 '{key}' に {value} を加算しました。現在値: {_variables[key]}");
    }

    public int GetInt(string key)
    {
        int value = _variables.ContainsKey(key) ? _variables[key] : 0;
        Debug.Log($"<color=orange>[StateManager: Get]</color> 変数 '{key}' を取得しました。現在値: {value}");
        return value;
    }
    
}