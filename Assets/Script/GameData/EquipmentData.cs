using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "ScriptableObjects/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private string name; // 名前
    [SerializeField] private string type; // 種別
    [SerializeField] private int attack; // 攻撃力
    [SerializeField] private int magicAttack; // 魔法攻撃力
    [SerializeField] private int defense; // 防御力
    [SerializeField] private int magicDefense; // 魔法防御力
    [SerializeField] private int speed; // 速さ
    [SerializeField] private int price; // 値段
    [SerializeField] private string magicType; // 属性タイプ
    [SerializeField] private string rarity; // レア度
    [SerializeField] private int explanation; // 説明

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public string Type => type;
    public int Attack => attack;
    public int MagicAttack => magicAttack;
    public int Defense => defense;
    public int MagicDefense => magicDefense;
    public int Speed => speed;
    public int Price => price;
    public string MagicType => magicType;
    public string Rarity => rarity;
    public int Explanation => explanation;
}
