using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private GameObject _healthMainPanel;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;
    
    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetupButtons();
        if (_healthMainPanel != null)
        {
            _healthMainPanel.SetActive(true);
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
        if (_healthMainPanel != null)
        {
            _healthMainPanel.SetActive(false);
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