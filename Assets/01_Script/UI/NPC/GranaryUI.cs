using UnityEngine;
using UnityEngine.UI;

public class GranaryUI : MonoBehaviour
{
    [SerializeField] private GameObject _granaryMainPanel;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetupButtons();
        if (_granaryMainPanel != null)
        {
            _granaryMainPanel.SetActive(true);
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
        if (_granaryMainPanel != null)
        {
            _granaryMainPanel.SetActive(false);
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