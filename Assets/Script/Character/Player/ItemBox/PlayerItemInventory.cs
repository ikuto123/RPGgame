using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerItemInventory : MonoBehaviour
{
    [SerializeField] private List<InventorySlot> inventory = new List<InventorySlot>();
    private int MaxAmonnt = 99;
    
    public static PlayerItemInventory Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void AddItem(ItemData item , int amount = 1)
    {
        //同じアイテムがあるのかを確認
        var existingSlot = inventory.FirstOrDefault(slot => slot.itemData == item);
        
        if (existingSlot != null) 
        {
            //超えないようにカウント
            existingSlot.quantity = Mathf.Min(existingSlot.quantity + amount, MaxAmonnt);
            
            //既存アイテムを増やす 
            existingSlot.AddItemQuantity(amount);
            
        }
        else
        {
            //新しくインベントリに追加する
            inventory.Add(new InventorySlot(item, Mathf.Min(amount, MaxAmonnt))); 
        }
        
        Debug.Log($"{item.Name} を {amount}個 入手しました。");
    }

    public bool TryUseItem(ItemData item, int amount = 1 , bool isUse = true)
    {
        var Slot = inventory.FirstOrDefault(slot => slot.itemData == item);

        if (Slot != null && Slot.quantity >= amount)
        {
            Slot.quantity -= amount;

            //アイテムが0になったらリストから消す
            if (Slot.quantity <= 0)
            {
                inventory.Remove(Slot);
            }
            return true;
        }
        return false;
    }

    public List<InventorySlot> ShowItemInventory()
    {
        return inventory;
    }
}
