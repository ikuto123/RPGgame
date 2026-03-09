using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentShopItemInfo
{
    public EquipmentData equipmentData; 
    public int pieces; 
}

public class ShopEquipment : MonoBehaviour
{
    //IDと価格を紐づけるためのリスト
    [SerializeField] private List<string> equipmentIdList = new List<string>();

    //アイテム本体を格納するリスト
    public List<EquipmentShopItemInfo> equipmentList = new List<EquipmentShopItemInfo>();
}