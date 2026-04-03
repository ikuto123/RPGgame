using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public enum EquipmentSlot
{
    Weapon = 1,
    Head = 0,
    Body = 2,
    Legs = 4,
    Feet = 6,
    Accessory1 = 3,
    Accessory2 = 5
}

//キャラクターステータスも見ろ!!!
public class PlayerStatus : CharacterStatus
{
    public static string GetEquipmentType(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => "武器",
            EquipmentSlot.Head => "防具_頭",
            EquipmentSlot.Body => "防具_体",
            EquipmentSlot.Legs => "防具_腰",
            EquipmentSlot.Feet => "防具_足",
            EquipmentSlot.Accessory1 => "アクセサリー",
            EquipmentSlot.Accessory2 => "アクセサリー",
        };
    }
    
    
    [SerializeField] private int level;
    [SerializeField] private int experience;
    [SerializeField] private int coin;

    public int Level => level;
    public int Experience => experience;
    public int Coin => coin;
    
    [SerializeField] PlayerData playerData;
    
    [Header("装備")]
    private Dictionary<EquipmentSlot, EquipmentData> equippedItems = new Dictionary<EquipmentSlot, EquipmentData>();
    
    private void Start()
    {
        HP = playerData.MaxHP;
        MP = playerData.MaxMP;
        currentHP = HP;
        currentMP = MP;
        attack = playerData.MaxAttack;
        defense = playerData.MaxDefense;
        level = 1;
        coin = 100;
    }
    
    private void Awake()
    {
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            equippedItems[slot] = null;
        }
    }
    
    private void status(int lvl)
    {
        
        
    }

    private (int,int) Status_Expression(int minStatus, int maxStatus , int maxLevel)
    {
        int minLevel = 1;
        int a = (maxStatus - minStatus) / (maxLevel - minLevel);
        int b = maxLevel - a * maxStatus;
        
        return (a , b);
    }
    public void UpCoin(int amount) => coin += Mathf.Min(amount,99999);

    public void DownCoin(int amount) => coin = Mathf.Max(coin - amount, 0);
    
    public void GainExperience(int amount)
    {
        experience += amount;
        if (experience >= GetExperienceForNextLevel())
        {
            experience -= GetExperienceForNextLevel();
            level += Mathf.Min(level + 1 , 99);
        }
    }

    private int GetExperienceForNextLevel()
    {
        return 0;
    }

    public void SetEquipment(EquipmentData equipmentData , EquipmentSlot slot)
    {
        equippedItems[slot] = equipmentData;
        
    }
    
    public EquipmentData GetEquippedItem(EquipmentSlot slot)
    {
        if (equippedItems.ContainsKey(slot)) return equippedItems[slot];
        
        return null;
    }
    
    public int TotalAttack
    {
        get
        {
            int total = Attack; // キャラクターの素の攻撃力
            foreach (var equip in equippedItems.Values)
            {
                if (equip != null) total += equip.Attack; // 装備品の攻撃力を足す
            }
            return total;
        }
    }

    public int TotalDefense
    {
        get
        {
            int total = Defense; // キャラクターの素の防御力
            foreach (var equip in equippedItems.Values)
            {
                if (equip != null) total += equip.Defense; // 装備品の防御力を足す
            }
            return total;
        }
    }
}