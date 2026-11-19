using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelAndStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _levelUpTabButton;
    [SerializeField] private Button _statsTabButton;

    [Header("Panels")]
    [SerializeField] private LevelPanel _levelPanel;
    [SerializeField] private StatsPanel _statsPanel;

    [Header("Currency References")]
    [SerializeField] private TextMeshProUGUI _txtCurrency;

    [Header("Navigation")]
    [SerializeField] private Selectable firstSelectedButton;

    public Selectable FirstSelectedButton => firstSelectedButton;

    private CurrencyManager currencyManager;

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf()
    {
        if (PlayerUI.Instance != null)
            PlayerUI.Instance.CloseAllRolePanels();
        else
            gameObject.SetActive(false);
    }

    public void ActivateLevelPanel() => _levelPanel?.ActivateSelf();
    public void DeactivateLevelPanel() => _levelPanel?.DeactivateSelf();

    public void ActivateStatsPanel() => _statsPanel?.ActivateSelf();
    public void DeactivateStatsPanel() => _statsPanel?.DeactivateSelf();

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        Unsubscribe();
        GameManager.Instance?.playerUI?.SetInputMode(false);
    }

    public void Initialize()
    {
        Unsubscribe();

        currencyManager = GameManager.Instance?.currencyManager;
        if (currencyManager != null)
            currencyManager.OnChange += UpdateCurrencyText;

        UpdateCurrencyText();
        ActivateLevelPanel();
        DeactivateStatsPanel();
        SubscribeButtons();
    }

    private void SubscribeButtons()
    {
        if (_backButton != null)
            _backButton.onClick.AddListener(DeactivateSelf);

        if (_levelUpTabButton != null)
            _levelUpTabButton.onClick.AddListener(ShowLevelPanel);

        if (_statsTabButton != null)
            _statsTabButton.onClick.AddListener(ShowStatsPanel);
    }

    private void Unsubscribe()
    {
        if (currencyManager != null)
            currencyManager.OnChange -= UpdateCurrencyText;

        if (_backButton != null)
            _backButton.onClick.RemoveListener(DeactivateSelf);

        if (_levelUpTabButton != null)
            _levelUpTabButton.onClick.RemoveListener(ShowLevelPanel);

        if (_statsTabButton != null)
            _statsTabButton.onClick.RemoveListener(ShowStatsPanel);
    }

    private void ShowLevelPanel()
    {
        ActivateLevelPanel();
        DeactivateStatsPanel();
    }

    private void ShowStatsPanel()
    {
        ActivateStatsPanel();
        DeactivateLevelPanel();
    }

    private void UpdateCurrencyText()
    {
        if (_txtCurrency != null && currencyManager != null)
            _txtCurrency.text = currencyManager.Money.ToString();
    }
}