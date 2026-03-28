using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; 
using System.Threading;

public enum SelectLayout 
{
    Vertical,   // 縦（デフォルト）
    Horizontal, // 横
    Grid        // 縦横（グリッド）
}

public class SelectInput
{
    private List<UISelectable> _items;
    private int _currentIndex;
    
    private float _nextInputTime;
    private const float InputInterval = 0.2f;
    private Action<int> _onSelectionChanged;
    
    public void ClearSelection()
    {
        if (_items != null && _items.Count > 0 && _currentIndex >= 0 && _currentIndex < _items.Count)
        {
            _items[_currentIndex].SetSelected(false);
        }
    }
    
    // 引数に canEscapeDown と canEscapeUp を追加（デフォルトは false なので既存のコードは壊れません）
    public async UniTask<int> WaitForSelection(
        List<UISelectable> items, 
        CancellationToken token, 
        Action<int> onSelectionChanged = null,
        int startIndex = 0, 
        SelectLayout layout = SelectLayout.Vertical, 
        int columns = 1,
        bool canEscapeDown = false, 
        bool canEscapeUp = false    
    )
    {
        _items = items;
        
        // アイテムが0個の時のエラーを防ぐための安全対策
        if (_items == null || _items.Count == 0) return -1;
        
        _currentIndex = Mathf.Clamp(startIndex, 0, _items.Count - 1);
        _onSelectionChanged = onSelectionChanged; // コールバックを保持
        
        RefreshSelection();
        
        //決定されるまで無限ループ
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            
            //決定キー
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                //決定時の演出をしてからインデックスを返す
                _items[_currentIndex].Press(); 
                return _currentIndex; 
            }
            if(Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape))
            {
                return -1; //キャンセルされた
            }

            //移動キー
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if (Time.time >= _nextInputTime && (v != 0 || h != 0))
            {
                int direction = 0;

                if (layout == SelectLayout.Vertical)
                {
                    if (v > 0) direction = -1;      // 上
                    else if (v < 0) direction = 1;  // 下
                    
                    if (direction == 0)
                    {
                        if (h > 0) direction = 1;
                        else if (h < 0) direction = -1;
                    }
                }
                else if (layout == SelectLayout.Horizontal)
                {
                    if (canEscapeDown && v < 0)
                    {
                        _nextInputTime = Time.time + InputInterval;
                        return -2;
                    } 

                    // 横移動のみ
                    if (h > 0) direction = 1;       // 右
                    else if (h < 0) direction = -1; // 左
                }
                else if (layout == SelectLayout.Grid)
                {
                    if (canEscapeUp && v > 0 && _currentIndex < columns)
                    {
                        _nextInputTime = Time.time + InputInterval;
                        return -3;
                    }

                    // 縦横両方
                    if (v > 0) direction = -columns;      // 上
                    else if (v < 0) direction = columns;  // 下
                    
                    // 上下入力がない場合のみ左右を判定
                    if (direction == 0)
                    {
                        // 横移動時は、行の端での意図しない折り返しを防ぐ
                        if (h > 0 && (_currentIndex % columns) < columns - 1) direction = 1;
                        else if (h < 0 && (_currentIndex % columns) > 0) direction = -1;
                    }
                }

                // directionが計算された場合のみ既存のChangeSelectionを呼ぶ
                if (direction != 0)
                {
                    ChangeSelection(direction);
                    _nextInputTime = Time.time + InputInterval;
                }
            }
        }

        return -1; //キャンセルされた
    }
    
    private void ChangeSelection(int direction)
    {
        _items[_currentIndex].SetSelected(false);
        
        _currentIndex += direction;
        if (_currentIndex >= _items.Count) _currentIndex = _items.Count - 1;
        if (_currentIndex < 0) _currentIndex = 0;

        RefreshSelection();
    }
    
    private void RefreshSelection()
    {
        if (_items.Count > 0)
        {
            for (int i = 0; i < _items.Count; i++)
                _items[i].SetSelected(false);
            
            _items[_currentIndex].SetSelected(true);
            _onSelectionChanged?.Invoke(_currentIndex);
        }
    }
}