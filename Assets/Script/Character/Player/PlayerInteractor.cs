using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float InteractionRange = 2f;
    [SerializeField] private LayerMask InteractableLayer;   
    [SerializeField] private Transform DetectionOrigin;
    
    private IInputProvider _inputProvider;
    private IInteractable _currentInteractable;
    
    private void Start()
    {
        _inputProvider = GetComponent<IInputProvider>();
        
        if (DetectionOrigin == null) DetectionOrigin = transform;
    }

    private void Update()
    {
        // 会話中はインタラクト操作（話しかけるなど）を禁止する
        // ※これが「制限」ですが、会話中に会話を重ねないために必要です
        if (UIManager.Instance != null && 
            UIManager.Instance.DialogueView != null && 
            UIManager.Instance.DialogueView.IsVisible) 
        {
            return; 
        }
        
        CheckForInteractable();
        HandleInput();
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(DetectionOrigin.position, DetectionOrigin.forward);
        RaycastHit hit;
        IInteractable newInteractable = null;

        //相手が選択でいるオブジェクトかどうかの判定
        if (Physics.Raycast(ray, out hit, InteractionRange, InteractableLayer))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        if (newInteractable != _currentInteractable)
        {
            _currentInteractable = newInteractable;
            
            var uiManager = UIManager.Instance;
            if (uiManager == null || uiManager.InteractionView == null) return;

            if (_currentInteractable != null)
            {
                //インタラクト可能なオブジェクトが見つかった場合、文字を表示
                string promptText = _currentInteractable.ShowInteractionText();

                if (string.IsNullOrEmpty(promptText))
                {
                    uiManager.InteractionView.Hide();
                }
                else
                {
                    //テキストをセットして、UIを表示させる
                    uiManager.InteractionView.SetPromptText(promptText);
                    uiManager.InteractionView.Show();
                }
            }
            else
            {
                uiManager.InteractionView.Hide();
            }
        }
    }

    private void HandleInput()
    {
        if (_currentInteractable != null && _inputProvider != null)
        {
            var input = _inputProvider.GetInput();
            if (input.IsInteract)
            {
                _currentInteractable.OnInteract(GetComponent<PlayerController>());
                UIManager.Instance.InteractionView.Hide();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (DetectionOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(DetectionOrigin.position, DetectionOrigin.forward * InteractionRange);
        }
    }
}