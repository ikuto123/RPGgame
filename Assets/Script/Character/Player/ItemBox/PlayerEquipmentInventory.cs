using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipmentInventory : MonoBehaviour
{
    public static PlayerEquipmentInventory Instance { get; private set; }
    
    [Header("容量制限")]
    [SerializeField] private int maxWeapons = 99;      
    [SerializeField] private int maxArmors = 99;       
    [SerializeField] private int maxAccessories = 99;
    
    [Header("所持リスト")]
    [SerializeField] private List<EquipmentData> weapons = new List<EquipmentData>();
    [SerializeField] private List<EquipmentData> armors = new List<EquipmentData>();
    [SerializeField] private List<EquipmentData> accessories = new List<EquipmentData>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public bool AddEquipment(EquipmentData equipment)
    {
        if (equipment == null) return false;

        switch (equipment.Type)
        {
            case "武器":
                if (weapons.Count >= maxWeapons) return false;
                weapons.Add(equipment);
                break;
            case "防具":
                if (armors.Count >= maxArmors) return false;
                armors.Add(equipment);
                break;
            case "アクセサリー":
                if (accessories.Count >= maxAccessories) return false;
                accessories.Add(equipment);
                break;
            default:
                return false;
        }

        return true;
    }
    
    public void RemoveEquipment(EquipmentData item)
    {
        switch (item.Type)
        {
            case "武器": weapons.Remove(item); break;
            case "防具": armors.Remove(item); break;
            case "アクセサリー": accessories.Remove(item); break;
        }
    }
    
}
