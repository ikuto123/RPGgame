using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DropItemData", menuName = "ScriptableObjects/DropItemData")]
public class DropItemData : ScriptableObject
{
    [SerializeField] private string id; // ID
    [SerializeField] private int categoryId; // カテゴリーId
    [SerializeField] private string enemyId; // エネミーID
    [SerializeField] private string type; // 種別
    [SerializeField] private string variousId; // 各種ID
    [SerializeField] private int pieces; // 個数
    [SerializeField] private float probability; // 確率(割合)

    public string Id => id;
    public int CategoryId => categoryId;
    public string EnemyId => enemyId;
    public string Type => type;
    public string VariousId => variousId;
    public int Pieces => pieces;
    public float Probability => probability;
}
