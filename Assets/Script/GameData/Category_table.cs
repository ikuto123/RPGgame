using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Category_table", menuName = "ScriptableObjects/Category_table")]
public class Category_table : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private string name; // 名前
    [SerializeField] private string name_English; // 名前(英語)

    public string Id => id;
    public string Name => name;
    public string Name_English => name_English;
}
