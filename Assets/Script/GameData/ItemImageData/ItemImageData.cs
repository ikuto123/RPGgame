using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDataUnit", menuName = "ScriptableObjects/ItemImageData")]
public class ItemImageData : ScriptableObject
{
    public List<Itemimage> ItemImageDataList = new List<Itemimage>();

    private Dictionary<string, Sprite> _imageMap;
    [System.NonSerialized] private bool _isInitialized = false;
    
    [System.Serializable]
    public class Itemimage
    {
        [SerializeField] private string itemImageName;
        [SerializeField] private Sprite itemImage;
        [SerializeField] private string itemId ;
    
        public Sprite ItemImage => itemImage;
        public string ItemId => itemId;
    }

    private void Initialize()
    {
        if(_isInitialized) return;
        _isInitialized = true;
        
        _imageMap = new Dictionary<string, Sprite>();
        foreach (var data in ItemImageDataList)
        {
            if(data == null || string.IsNullOrEmpty(data.ItemId)) continue;

            if (!_imageMap.ContainsKey(data.ItemId))
            {
                _imageMap.Add(data.ItemId, data.ItemImage);
            }
        }
    }

    public Sprite GetSprite(string id)
    {
        Initialize();
        if (_imageMap.TryGetValue(id, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }


}