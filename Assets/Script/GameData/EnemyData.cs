using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private string name; // 名前
    [SerializeField] private int maxHP; // HP
    [SerializeField] private int maxMP; // MP
    [SerializeField] private int attack; // 攻撃
    [SerializeField] private int magicAttack; // 魔法攻撃
    [SerializeField] private int defense; // 防御
    [SerializeField] private int magicDefense; // 魔法防御
    [SerializeField] private int dropExp; // ドロップ経験値
    [SerializeField] private int speed; // 早さ
    [SerializeField] private string magicType; // 魔法タイプ

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public int MaxHP => maxHP;
    public int MaxMP => maxMP;
    public int Attack => attack;
    public int MagicAttack => magicAttack;
    public int Defense => defense;
    public int MagicDefense => magicDefense;
    public int DropExp => dropExp;
    public int Speed => speed;
    public string MagicType => magicType;
}
