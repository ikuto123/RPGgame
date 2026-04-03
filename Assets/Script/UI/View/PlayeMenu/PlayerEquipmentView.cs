using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEquipmentView : UIViewBase
{
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private PlayerStatus playerStatus;
    
    [SerializeField] private GameObject _inventoryPanel; // アイテム一覧を表示する親パネル（非選択時は非表示にする想定）
    [SerializeField] private GameObject _equipmentItemBox; // PlayerItemViewの _playerItembox に相当するGridオブジェクト

    [Header("Data References")]
    [SerializeField] private PlayerEquipmentInventory playerEquipmentInventory;
    [SerializeField] private ItemImageData itemImageData;
    
    [Header("Scroll References")]
    [SerializeField] private RectTransform _viewportRect; // スクロールの見える範囲 (Viewport)
    [SerializeField] private RectTransform _contentRect;  // スクロールで動かす中身 (_equipmentItemBox など)
    [SerializeField] private float topPadding = 20f;
    [SerializeField] private float bottomPadding = 20f;
    private Vector2 _initialContentPos;

    // スロット選択用
    private List<UISelectable> _SelectEquipmentButtons = new List<UISelectable>();
    private SelectInput _selectInput = new SelectInput();
    private int _currentIndex = 0;

    // インベントリ選択用
    private SelectInput _inventorySelectInput = new SelectInput();
    private List<UISelectable> _inventoryItems = new List<UISelectable>();
    private List<EquipmentData> _currentEquipmentList = new List<EquipmentData>();
    private int _inventoryIndex = 0;
    
    private EquipmentSlot _currentSelectedSlot;

    private void Awake()
    {
        _SelectEquipmentButtons = new List<UISelectable>();

        for (int i = 0; i < equipmentPanel.transform.childCount; i++)
        {
            Transform child = equipmentPanel.transform.GetChild(i);

            if (child.TryGetComponent<UISelectable>(out var selectable))
            {
                _SelectEquipmentButtons.Add(selectable);
                // Debug.Log($"Add : {selectable.name}");
            }
        }
        
        if (_contentRect != null)
        {
            _initialContentPos = _contentRect.anchoredPosition;
        }

        _inventoryPanel.SetActive(false);
        Hide();
    }
    
    public void ShowPreview()
    {
        Show();
    }

    private void Show()
    {
        base.Show();
        UIManager.Instance.PlayerStatusView.UpdateStatus();
    }
    public async UniTask<bool> StartSelectionAsync(PlayerItemMenuMode mode, CancellationToken token)
    {
        if (mode == PlayerItemMenuMode.Sell) return true;

        while (!token.IsCancellationRequested)
        {
            int r = await _selectInput.WaitForSelection(
                _SelectEquipmentButtons, 
                token,
                onSelectionChanged: null, 
                startIndex: _currentIndex, 
                layout: SelectLayout.Grid, 
                columns: 2, 
                canEscapeDown: false, 
                canEscapeUp: true
            );
            
            _selectInput.ClearSelection();

            // -1: キャンセル(B/Esc), -3: 上キー（タブに戻る）
            if (r == -1 || r == -3)
            {
                return true;
            }

            // 選択されたインデックスを更新
            _currentIndex = r;

            Debug.Log($"装備スロット {_currentIndex} 番が選択されました");
            await OpenInventorySelectionAsync(_currentIndex, token);
            
            // TODO: 必要に応じて装備更新後のUI反映処理などを呼ぶ
            PlayerEquip(_currentIndex);
        }
        
        return true;
    }
    
    private void PlayerEquip(int index)
    {
        EquipmentSlot slot = (EquipmentSlot)index;
        string equipment = PlayerStatus.GetEquipmentType(slot);

        
        Debug.Log($"選択されたスロット: {slot}");
    }
 
    
    private async UniTask OpenInventorySelectionAsync(int slotIndex, CancellationToken token)
    {
        EquipmentSlot slot = (EquipmentSlot)slotIndex;
        _currentSelectedSlot = slot;
        
        // 1. スロットに応じたインベントリのリストを取得してUIにセット
        bool hasItems = SetEquipmentInventoryData(slot);
        if (!hasItems)
        {
            Debug.Log("このスロットに装備できるアイテムがありません。");
            return; 
        }

        // 2. インベントリUIを表示して初期選択を更新
        if (_inventoryPanel != null) _inventoryPanel.SetActive(true);
        _inventoryIndex = 0;
        
        // パネルを開くたびにスクロール位置を一番上に戻す
        ResetScroll();
        OnInventorySelectionChanged(_inventoryIndex);

        // 3. インベントリ側の選択待機ループ
        while (!token.IsCancellationRequested)
        {
            int r = await _inventorySelectInput.WaitForSelection(
                _inventoryItems, 
                token,
                onSelectionChanged: OnInventorySelectionChanged, // コールバックを再設定
                startIndex: _inventoryIndex, 
                layout: SelectLayout.Grid, 
                columns: 5, 
                canEscapeDown: false, 
                canEscapeUp: false
            );

            _inventorySelectInput.ClearSelection();

            // -1: キャンセル
            if (r == -1 || r == -3)
            {
                break; // ループを抜けてスロット選択（親ループ）に戻る
            }

            _inventoryIndex = r;

            // 決定時の処理
            EquipmentData selectedEquip = _currentEquipmentList[_inventoryIndex];
            Debug.Log($"{selectedEquip.Name} を装備しました");
            
            // 装備処理
            playerStatus.SetEquipment(selectedEquip, slot);
            UpdateSlotIcon(slotIndex, selectedEquip);
            
            break; // 装備が完了したらインベントリを閉じてスロット選択に戻る
        }

        // 4. インベントリUIを非表示にする
        if (_inventoryPanel != null) _inventoryPanel.SetActive(false);
        UIManager.Instance.PlayerStatusView.UpdateStatus();
        
    }


    private bool SetEquipmentInventoryData(EquipmentSlot slot)
    {
        foreach (Transform child in _equipmentItemBox.transform) 
            child.gameObject.SetActive(false);

        string targetType = PlayerStatus.GetEquipmentType(slot);
        List<EquipmentData> baseList = new List<EquipmentData>();

        if (targetType == "武器") 
        {
            baseList = playerEquipmentInventory.GetWeaponsInventory();
        }
        else if (targetType.StartsWith("防具")) 
        {
            baseList = playerEquipmentInventory.GetArmorsInventory();
        }
        else if (targetType == "アクセサリー") 
        {
            baseList = playerEquipmentInventory.GetAccessoriesInventory();
        }

        _currentEquipmentList = baseList.Where(equip => equip.Type == targetType).ToList();

        if (_currentEquipmentList == null || _currentEquipmentList.Count == 0)
            return false;

        for (int i = 0; i < _currentEquipmentList.Count; i++)
        {
            if (i >= _equipmentItemBox.transform.childCount) break;

            Transform child = _equipmentItemBox.transform.GetChild(i);
            child.gameObject.SetActive(true);
            
            child.GetChild(0).GetComponent<Image>().sprite = itemImageData.GetSprite(_currentEquipmentList[i].Id);
            child.GetChild(1).GetComponent<TextMeshProUGUI>().text = "1";
        }

        _inventoryItems = _equipmentItemBox.GetComponentsInChildren<UISelectable>(true)
            .Where(item => item.gameObject.activeSelf).ToList();

        return true;
    }
    
    private void OnInventorySelectionChanged(int index)
    {
        if (_currentEquipmentList == null || _currentEquipmentList.Count <= index) return;

        Scroll(index);

        // ★追加：カーソルが合っているアイテムと、現在装備中のアイテムを取得
        EquipmentData selectedEquip = _currentEquipmentList[index];
        EquipmentData currentEquip = playerStatus.GetEquippedItem(_currentSelectedSlot);

        // ★追加：プレビューUIを更新する
        if (UIManager.Instance != null && UIManager.Instance.PlayerStatusView != null)
        {
            UIManager.Instance.PlayerStatusView.UpdatePreviewStatus(currentEquip, selectedEquip);
        }
    }
    
    private void ResetScroll()
    {
        if (_contentRect != null) _contentRect.anchoredPosition = _initialContentPos;
    }

    private void Scroll(int index)
    {
        if (_viewportRect == null || _contentRect == null || _inventoryItems == null || _inventoryItems.Count <= index) return;
        
        if (index == 0) 
        { 
            ResetScroll(); 
            return; 
        }

        Canvas.ForceUpdateCanvases();
        RectTransform selectedRect = _inventoryItems[index].GetComponent<RectTransform>();
        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] itemCorners = new Vector3[4];

        _viewportRect.GetWorldCorners(viewportCorners);
        selectedRect.GetWorldCorners(itemCorners);

        float itemBottom = itemCorners[0].y;
        float itemTop = itemCorners[1].y;
        float viewportTop = viewportCorners[1].y - topPadding;
        float viewportBottom = viewportCorners[0].y + bottomPadding;

        // PlayerItemViewと完全に同じ縦の移動計算
        if (itemTop > viewportTop)
        {
            _contentRect.position -= new Vector3(0, itemTop - viewportTop, 0);
        }
        else if (itemBottom < viewportBottom)
        {
            _contentRect.position += new Vector3(0, viewportBottom - itemBottom, 0);
        }

        // ★ここが重要：スクロール計算の直後に、X座標(横)だけを初期位置に強制的に戻す
        // これにより、Unityの仕様で横にズレようとしても絶対にズレなくなります
        _contentRect.anchoredPosition = new Vector2(_initialContentPos.x, _contentRect.anchoredPosition.y);
    }

    private void UpdateSlotIcon(int slotIndex, EquipmentData equip)
    {
        if (slotIndex >= _SelectEquipmentButtons.Count) return;

        // ボタン自身のImageコンポーネントを取得する
        // （※もし白い四角の画像が子オブジェクトにある場合は GetComponentInChildren<Image>() などに変更してください）
        Image slotImage = _SelectEquipmentButtons[slotIndex].gameObject.transform.GetChild(0).GetComponent<Image>();

        if (slotImage != null)
        {
            if (equip != null)
            {
                // アイテムの画像をセット
                slotImage.sprite = itemImageData.GetSprite(equip.Id);
                slotImage.color = Color.white; // 透明度などをリセット
            }
            else
            {
                // 装備を外した場合は画像を消す（透明にするか、元の白い画像に戻す）
                slotImage.sprite = null;
            }
        }
    }
    
    
}
