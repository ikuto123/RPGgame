using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DecorationData", menuName = "ScriptableObjects/DecorationData")]
public class DecorationData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private string name; // 名前
    [SerializeField] private string location; // 設置個所
    [SerializeField] private string effectType; // 効果タイプ
    [SerializeField] private int amount; // 効果量(割合)
    [SerializeField] private int exp; // 経験値
    [SerializeField] private string rarity; // レア度
    [SerializeField] private int price; // 値段
    [SerializeField] private int explanation; // 説明

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public string Location => location;
    public string EffectType => effectType;
    public int Amount => amount;
    public int Exp => exp;
    public string Rarity => rarity;
    public int Price => price;
    public int Explanation => explanation;
}
