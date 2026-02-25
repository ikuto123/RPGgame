using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks; 

public class DialogueView : UIViewBase
{
    [Header("UI References")]
    public TextMeshProUGUI NameText;     
    public TextMeshProUGUI BodyText;     
    public Image NextCursor;             
    public Transform ChoiceContainer;    
    
    [Header("Manager References")]
    public PortraitManager PortraitMgr;  

    [Header("Settings")]
    public float TypeSpeed = 0.05f;      
    [SerializeField] private ItemImageData _imageData; 

    private DialogueRunner _activeRunner;
    private string _fullText;
    private string _autoNextPortName; 

    private SelectInput _selectInput = new SelectInput(); // ※SelectInputの実装に依存しますがそのままにします
    private Queue<RewardEntry> _rewardQueue = new Queue<RewardEntry>();
    private CancellationTokenSource _cts;
    private List<UISelectable> _choiceSelectables = new List<UISelectable>();
    
    // 表示処理（DialogueRunnerから呼ばれる）
    public void Show(DialogueRunner runner)
    {
        base.Show();
        _activeRunner = runner;
        
        // 会話開始時はカーソルを消す
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetCursorState(false);
            UIManager.Instance.PlayerStop = true;
        }
    }

    public override void Hide()
    {
        base.Hide(); 
        if (PortraitMgr != null) PortraitMgr.ResetPortraits();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetCursorState(false);
            UIManager.Instance.PlayerStop = false;
        }
    }

    // ノード表示のメイン処理
    public async UniTask DisplayNode(DialogueNodeData node, List<NodeLinkData> choices)
    {
        // 1. キャンセル処理と初期化
        CancelCurrentTasks();
        _cts = new CancellationTokenSource();
        
        var data = node.EventData;
        
        // UI初期化
        if (NameText != null) NameText.text = data.SpeakerName;
        _fullText = data.DialogueText;
        if (BodyText != null) BodyText.text = "";
        if (NextCursor != null) NextCursor.gameObject.SetActive(false);
        
        // 立ち絵更新
        if (PortraitMgr != null) PortraitMgr.UpdatePortraits(data.LeftPortrait, data.RightPortrait, data.SpeakerSide);

        // 選択肢ボタンのリセット
        foreach (Transform child in ChoiceContainer) child.gameObject.SetActive(false);
        
        // 報酬データの準備
        _rewardQueue.Clear();
        if (data.Rewards != null && data.Rewards.Count > 0)
        {
            foreach (var reward in data.Rewards)
            {
                if ((reward.Type == RewardType.Item && reward.Item != null) ||
                    (reward.Type == RewardType.Equipment && reward.Equipment != null))
                {
                    _rewardQueue.Enqueue(reward);
                }
            }
        }

        // 2. テキスト表示（文字送り待機）
        await StartText(_fullText, _cts.Token);
        
        // 3. 入力待ち（選択肢 or 決定キー）
        _autoNextPortName = null; // 初期化

        if (choices != null && choices.Count > 1)
        {
            // --- 分岐がある場合 ---
            CreateChoiceButtons(choices);
            
            //選択を待つ関数
            await _selectInput.WaitForSelection(_choiceSelectables, _cts.Token);
        }
        else
        {
            // --- 一本道の場合 ---
            if (choices != null && choices.Count == 1)
            {
                _autoNextPortName = choices[0].PortName;
            }

            // カーソルを表示して入力待ち
            if (NextCursor != null) NextCursor.gameObject.SetActive(true);
            await InputWait(_cts.Token);
            if (NextCursor != null) NextCursor.gameObject.SetActive(false);
        }

        // 4. 報酬の受け取り処理（ここが終わるまで待機）
        await ProcessRewards();

        // 5. 次のノードへ進む
        ProceedToNextNode();
    }

    // 報酬を順番に処理するメソッド（再帰ではなくループに変更）
    private async UniTask ProcessRewards()
    {
        while (_rewardQueue.Count > 0)
        {
            var reward = _rewardQueue.Dequeue();
            string rName = "";
            Sprite rIcon = null;
            int rAmount = 1;

            bool showPopup = false;

            // アイテム・装備の判定
            if (reward.Type == RewardType.Item)
            {
                PlayerItemInventory.Instance.AddItem(reward.Item, reward.Amount);
                rName = reward.Item.Name; 
                rAmount = reward.Amount;
                if (_imageData != null) rIcon = _imageData.GetSprite(reward.Item.Id); 
                showPopup = true;
            }
            else if (reward.Type == RewardType.Equipment)
            {
                bool success = PlayerEquipmentInventory.Instance.AddEquipment(reward.Equipment);
                if (success)
                {
                    rName = reward.Equipment.Name;
                    if (_imageData != null) rIcon = _imageData.GetSprite(reward.Equipment.Id);
                    showPopup = true;
                }
            }
            
            // ポップアップを表示して、閉じるのを待つ
            if (showPopup && UIManager.Instance.ItemRewardView != null)
            {
                bool isClosed = false;
                
                // ShowRewardを呼び出し、コールバックでフラグを立てる
                await UIManager.Instance.ItemRewardView.ShowReward(rName, rIcon, rAmount, () => 
                { 
                    isClosed = true; 
                });
                
            }
        }
    }
    
    private void ProceedToNextNode()
    {
        if (_activeRunner != null)
        {
            _activeRunner.Proceed(_autoNextPortName);
        }
    }

    private void CreateChoiceButtons(List<NodeLinkData> choices)
    {
        ResetChoiceButtons();
        int available = ChoiceContainer.childCount;
        int count = Mathf.Min(choices.Count, available);
        
        for (int i = 0; i < count; i++)
        {
            var choice = choices[i];
            string portName = choice.PortName; 

            var go = ChoiceContainer.GetChild(i).gameObject;
            go.SetActive(true);

            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = portName;

            var selectable = go.GetComponent<UISelectable>();
            if (selectable == null) selectable = go.AddComponent<UISelectable>();
            _choiceSelectables.Add(selectable);

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.RemoveAllListeners(); 
                
                // キャプチャ問題対策（ローカル変数にコピー）
                string targetPort = portName;
                
                btn.onClick.AddListener(() =>
                {
                    // ボタンが押されたらポート名を保存(選択された場合に保存される)
                    _autoNextPortName = targetPort;
                });
            }
        }
    }

    private async UniTask StartText(string fullText, CancellationToken token)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        
        var typingTask = TypeLine(fullText, linkedCts.Token).SuppressCancellationThrow();        
        var skipInputTask = InputWait(linkedCts.Token).SuppressCancellationThrow();
        
        // どちらかが終わるのを待つ
        await UniTask.WhenAny(typingTask, skipInputTask);
        
        // キャンセル（入力を停止）
        linkedCts.Cancel();
        
        // 確実に全文を表示状態にする
        if (BodyText != null) BodyText.text = fullText;
    }

    private async UniTask TypeLine(string line, CancellationToken token)
    {
        if (BodyText != null) BodyText.text = "";

        foreach (char c in line.ToCharArray())
        {
            if (token.IsCancellationRequested) return;
            
            if (BodyText != null) BodyText.text += c;
            await UniTask.Delay(TimeSpan.FromSeconds(TypeSpeed), cancellationToken: token);
        }
    }
    
    private async UniTask InputWait(CancellationToken token)
    {
        await UniTask.WaitUntil(() => 
                Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || 
                Input.GetKeyDown(KeyCode.E), 
            cancellationToken: token);
    }
    
    private void ResetChoiceButtons()
    {
        _choiceSelectables.Clear();

        for (int i = 0; i < ChoiceContainer.childCount; i++)
        {
            var child = ChoiceContainer.GetChild(i);
            child.gameObject.SetActive(false);

            var btn = child.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                btn.onClick.RemoveAllListeners(); 
            }
            
            var selectable = child.GetComponent<UISelectable>();
            if (selectable != null) selectable.SetSelected(false);
        }
    }

    private void CancelCurrentTasks()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}