using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Magic_Attribute_Table", menuName = "ScriptableObjects/Magic_Attribute_Table")]
public class Magic_Attribute_Table : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private string name; // 名前
    [SerializeField] private string none; // なし
    [SerializeField] private string fire; // 炎
    [SerializeField] private string water; // 水
    [SerializeField] private string wind; // 風
    [SerializeField] private string thunder; // 雷
    [SerializeField] private string holy; // 光
    [SerializeField] private string evil; // 闇

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public string None => none;
    public string Fire => fire;
    public string Water => water;
    public string Wind => wind;
    public string Thunder => thunder;
    public string Holy => holy;
    public string Evil => evil;
}
