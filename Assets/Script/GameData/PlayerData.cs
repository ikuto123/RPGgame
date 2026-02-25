using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private string name; // 名前
    [SerializeField] private int minHP; // 最小HP
    [SerializeField] private int maxHP; // 最大HP
    [SerializeField] private int minMP; // 最小MP
    [SerializeField] private int maxMP; // 最大MP
    [SerializeField] private int minAttack; // 最小攻撃
    [SerializeField] private int maxAttack; // 最大攻撃
    [SerializeField] private int minMagicAttack; // 最小魔法攻撃
    [SerializeField] private int maxMagicAttack; // 最大魔法攻撃
    [SerializeField] private int minDefense; // 最小防御
    [SerializeField] private int maxDefense; // 最大防御
    [SerializeField] private int minMagicDefense; // 最小魔法防御
    [SerializeField] private int maxMagicDefense; // 最大魔法防御
    [SerializeField] private int speed; // 早さ
    [SerializeField] private int exp; // 必要経験値(Level1時)
    [SerializeField] private int maxLevel; // 最大レベル
    [SerializeField] private string job; // 職業
    [SerializeField] private string magicType; // 魔法タイプ

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public int MinHP => minHP;
    public int MaxHP => maxHP;
    public int MinMP => minMP;
    public int MaxMP => maxMP;
    public int MinAttack => minAttack;
    public int MaxAttack => maxAttack;
    public int MinMagicAttack => minMagicAttack;
    public int MaxMagicAttack => maxMagicAttack;
    public int MinDefense => minDefense;
    public int MaxDefense => maxDefense;
    public int MinMagicDefense => minMagicDefense;
    public int MaxMagicDefense => maxMagicDefense;
    public int Speed => speed;
    public int Exp => exp;
    public int MaxLevel => maxLevel;
    public string Job => job;
    public string MagicType => magicType;
}
