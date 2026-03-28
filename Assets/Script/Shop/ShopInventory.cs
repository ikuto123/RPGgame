using System.Collections.Generic;
using UnityEngine;

// ショップの種類を定義
public enum ShopType
{
    Item,      // 消費アイテム・素材屋
    Equipment  // 武器・防具屋
}

[System.Serializable]
public class ShopItemInfo
{
    public ItemData itemData; 
    public int pieces; // 在庫
}

[System.Serializable]
public class EquipmentShopItemInfo
{
    public EquipmentData equipmentData; 
    public int pieces; // 在庫
}

public class ShopInventory : MonoBehaviour
{
    [Header("ショップの種類を選択")]
    public ShopType shopType;

    // Item屋の場合はこちらを設定
    public List<ShopItemInfo> itemList = new List<ShopItemInfo>();
    
    // Equipment屋の場合はこちらを設定
    public List<EquipmentShopItemInfo> equipmentList = new List<EquipmentShopItemInfo>();
}