using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _shopMainPanel;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;
    
    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetupButtons();
        if (_shopMainPanel != null)
        {
            _shopMainPanel.SetActive(true);
        }
    }

    private void SetupButtons()
    {
        if (_exitButton != null)
        {
            _exitButton.onClick.RemoveAllListeners();
            _exitButton.onClick.AddListener(CloseUI);
        }
    }
    
    public void CloseUI()
    {
        if (_shopMainPanel != null)
        {
            _shopMainPanel.SetActive(false);
        }

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.CloseAllRolePanels();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}