using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentShopItemInfo
{
    public EquipmentData equipmentData; // アイテムのデータ
    public int pieces; // 値段や在庫数など（用途に合わせて名前を変えてください）
}

public class ShopEquipment : MonoBehaviour
{
    //IDと価格を紐づけるためのリスト
    [SerializeField] private List<string> equipmentIdList = new List<string>();

    //アイテム本体を格納するリスト
    public List<EquipmentShopItemInfo> equipmentList = new List<EquipmentShopItemInfo>();
}