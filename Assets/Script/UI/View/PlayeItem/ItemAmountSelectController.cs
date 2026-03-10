using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

// MonoBehaviourを継承しない純粋なC#クラス（個数選択ロジックだけを担当）
public class ItemAmountSelectController
{
    private GameObject _confirmationPopup;
    private TextMeshProUGUI _sellText; // 買う個数/売る個数を表示するテキスト
    private List<UISelectable> _popUpItems;
    private GameObject _sellCountGroup; // 個数選択UI全体をまとめるオブジェクト

    // コンストラクタ
    public ItemAmountSelectController(
        GameObject confirmationPopup, 
        TextMeshProUGUI sellText, 
        List<UISelectable> popUpItems,
        GameObject sellCountGroup) 
    {
        _confirmationPopup = confirmationPopup;
        _sellText = sellText;
        _popUpItems = popUpItems;
        _sellCountGroup = sellCountGroup;
    }

    // ポップアップを開いて、選ばれた個数を返す（キャンセル時は0）
    public async UniTask<int> WaitForConfirmationAsync(int maxAmount, CancellationToken token)
    {
        _confirmationPopup.SetActive(true);
        int currentAmount = 1;

        if (_popUpItems.Count < 5)
        {
            Debug.LogError("ポップアップのUI要素が5つ揃っていません！");
            _confirmationPopup.SetActive(false);
            return 0;
        }

        // 複数個選べるかどうかの判定
        bool canSelectAmount = maxAmount > 1;
        
        if (_sellCountGroup != null)
        {
            // 1個しかない場合は、白枠や矢印のUIをまるごと非表示にする
            _sellCountGroup.SetActive(canSelectAmount);
        }

        // 複数あるなら白枠(0)から、1個だけなら決定ボタン(3)からスタートする
        int currentIndex = canSelectAmount ? 0 : 3; 
        float nextInputTime = 0f;

        foreach (var item in _popUpItems) item.SetSelected(false);
        _popUpItems[currentIndex].SetSelected(true);

        while (!token.IsCancellationRequested)
        {
            if (_sellText != null) _sellText.text = currentAmount.ToString();

            await UniTask.Yield();

            // === 決定キーの処理 ===
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                _popUpItems[currentIndex].Press(); 

                if (currentIndex == 1) // 上矢印
                {
                    currentAmount++;
                    if (currentAmount > maxAmount) currentAmount = maxAmount;
                }
                else if (currentIndex == 2) // 下矢印
                {
                    currentAmount--;
                    if (currentAmount < 1) currentAmount = 1;
                }
                else if (currentIndex == 3) // 決定（買う/売る）
                {
                    _confirmationPopup.SetActive(false);
                    return currentAmount; 
                }
                else if (currentIndex == 4) // やめる
                {
                    _confirmationPopup.SetActive(false);
                    return 0; 
                }
            }

            // === キャンセルキーの処理 ===
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B))
            {
                _confirmationPopup.SetActive(false);
                return 0;
            }

            // === 十字キーのカスタム移動処理 ===
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if (Time.time >= nextInputTime && (v != 0 || h != 0))
            {
                int nextIndex = currentIndex;

                if (canSelectAmount)
                {
                    if (v > 0.5f) {
                        if (currentIndex == 3) nextIndex = 0;      
                        else if (currentIndex == 4) nextIndex = 2; 
                        else if (currentIndex == 2) nextIndex = 1; 
                    } else if (v < -0.5f) {
                        if (currentIndex == 0) nextIndex = 3;      
                        else if (currentIndex == 1) nextIndex = 2; 
                        else if (currentIndex == 2) nextIndex = 4; 
                    } else if (h > 0.5f) {
                        if (currentIndex == 0) nextIndex = 1;      
                        else if (currentIndex == 3) nextIndex = 4; 
                    } else if (h < -0.5f) {
                        if (currentIndex == 1 || currentIndex == 2) nextIndex = 0; 
                        else if (currentIndex == 4) nextIndex = 3;                 
                    }
                }
                else
                {
                    // 1個しかない場合は左右で「決定(3)」と「やめる(4)」だけを行き来
                    if (h > 0.5f && currentIndex == 3) nextIndex = 4;
                    else if (h < -0.5f && currentIndex == 4) nextIndex = 3;
                }

                if (nextIndex != currentIndex)
                {
                    _popUpItems[currentIndex].SetSelected(false);
                    currentIndex = nextIndex;
                    _popUpItems[currentIndex].SetSelected(true);
                    nextInputTime = Time.time + 0.2f; 
                }
            }
        }

        return 0;
    }
}