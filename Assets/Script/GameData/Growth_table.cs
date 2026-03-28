using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Growth_table", menuName = "ScriptableObjects/Growth_table")]
public class Growth_table : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private string name; // 職業
    [SerializeField] private int categoryId; // カテゴリーID
    [SerializeField] private float maxHP_ratio; // HP
    [SerializeField] private float maxMP_ratio; // MP
    [SerializeField] private float attack_ratio; // 攻撃
    [SerializeField] private float magicAttack_ratio; // 魔法攻撃
    [SerializeField] private float defense_ratio; // 防御
    [SerializeField] private float magicDefense_ratio; // 魔法防御

    public string Id => id;
    public string Name => name;
    public int CategoryId => categoryId;
    public float MaxHP_ratio => maxHP_ratio;
    public float MaxMP_ratio => maxMP_ratio;
    public float Attack_ratio => attack_ratio;
    public float MagicAttack_ratio => magicAttack_ratio;
    public float Defense_ratio => defense_ratio;
    public float MagicDefense_ratio => magicDefense_ratio;
}
