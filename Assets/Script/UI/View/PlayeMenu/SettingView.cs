using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Debug = UnityEngine.Debug;

public class SettingView : UIViewBase
{
    [SerializeField] private GameObject settingButtons;

    private List<UISelectable> _settingSelectables = new List<UISelectable>();
    private SelectInput _selectInput = new SelectInput();

    private int _settingIndex = 0;

    private void Awake()
    {
        RefreshSelectables();
        base.Hide();
    }

    public void ShowPreview()
    {
        base.Show();
        RefreshSelectables();
        _settingIndex = Mathf.Clamp(_settingIndex, 0, Mathf.Max(0, _settingSelectables.Count - 1));
    }

    private void RefreshSelectables()
    {
        _settingSelectables = settingButtons
            .GetComponentsInChildren<UISelectable>(true)
            .Where(x => x.gameObject.activeInHierarchy)
            .ToList();
        
    }

    public async UniTask<bool> StartSelectSetting(CancellationToken token)
    {
        Show();
        RefreshSelectables();

        if (_settingSelectables.Count == 0)
        {
            Debug.LogError("SettingView: 選択可能なUISelectableがありません");
            return true;
        }

        _settingIndex = Mathf.Clamp(_settingIndex, 0, _settingSelectables.Count - 1);

        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            int r = await _selectInput.WaitForSelection(
                _settingSelectables,
                token,
                startIndex: _settingIndex,
                layout: SelectLayout.Vertical,
                columns: 1,
                canEscapeUp: true,
                canEscapeDown: false
            );

            Debug.Log($"SettingView result = {r}");
            _selectInput.ClearSelection();

            if (r == -1 || r == -3)
            {
                return true;
            }

            _settingIndex = r;

            switch (r)
            {
                case 0:
                    // セーブ
                    break;
                case 1:
                    // ロード
                    break;
                case 2:
                    // タイトルへ戻る
                    break;
                case 3:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                default:
                    Debug.LogError("選択肢に処理を書いていないか、想定外の選択肢です");
                    break;
            }
        }

        return false;
    }
}