using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーId
    [SerializeField] private string name; // 名前
    [SerializeField] private string type; // 種別
    [SerializeField] private string effectType; // 効果タイプ
    [SerializeField] private float amount; // 効果量(割合)
    [SerializeField] private string rarity; // レア度
    [SerializeField] private int price; // 値段
    [SerializeField] private string magicType; // 属性タイプ
    [SerializeField] private string explanation; // 説明

    public string Id => id;
    public int CategoryId => categoryId;
    public string Name => name;
    public string Type => type;
    public string EffectType => effectType;
    public float Amount => amount;
    public string Rarity => rarity;
    public int Price => price;
    public string MagicType => magicType;
    public string Explanation => explanation;
}
