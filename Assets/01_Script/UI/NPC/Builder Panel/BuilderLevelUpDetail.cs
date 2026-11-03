using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderLevelUpDetail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private TextMeshProUGUI _levelUpCostText;

    [Header("Current Level Preview")]
    [SerializeField] private TextMeshProUGUI _currentLevelText;
    [SerializeField] private TextMeshProUGUI _currentHealthText;
    [SerializeField] private SliderBar _currentHealthBar;

    [Header("Next Level Preview")]
    [SerializeField] private TextMeshProUGUI _nextLevelText;
    [SerializeField] private TextMeshProUGUI _nextHealthText;
    [SerializeField] private SliderBar _nextHealthBar;

    private BuildingBase _granary;
    private CurrencyManager _currencyManager;

    private void Awake()
    {
        if (_levelUpButton != null)
            _levelUpButton.onClick.AddListener(HandleLevelUpButton);
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        if (_levelUpButton != null)
            _levelUpButton.onClick.RemoveListener(HandleLevelUpButton);
    }

    /// <summary>
    /// Sets the granary shown by this detail view and refreshes the display.
    /// </summary>
    public void Initialize(BuildingBase granary)
    {
        Unsubscribe();
        _granary = granary;

        if (isActiveAndEnabled)
            Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        _currencyManager = GameManager.Instance != null ? GameManager.Instance.currencyManager : null;

        if (_currencyManager != null)
            _currencyManager.OnChange += Refresh;

        if (_granary != null)
            _granary.OnHealthChanged += HandleHealthChanged;
    }

    private void Unsubscribe()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= Refresh;

        if (_granary != null)
            _granary.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        Refresh();
    }

    /// <summary>
    /// Pays the level up price and levels up the granary. The money is refunded if it fails.
    /// </summary>
    private void HandleLevelUpButton()
    {
        if (_granary == null || _currencyManager == null || !_granary.CanLevelUp)
        {
            Refresh();
            return;
        }

        int price = _granary.GetLevelUpPrice();
        if (!_currencyManager.SpendMoney(price))
        {
            Refresh();
            return;
        }

        if (!_granary.LevelUp())
            _currencyManager.AddMoney(price);

        Refresh();
    }

    private void Refresh()
    {
        bool hasGranary = _granary != null;
        bool isMax = !hasGranary || _granary.IsMaxLevel;

        int currentLevel = hasGranary ? _granary.Level : 1;
        int nextLevel = hasGranary ? Mathf.Min(currentLevel + 1, _granary.MaxLevel) : 1;

        int currentMaxHealth = hasGranary ? _granary.MaxHealth : 0;
        int currentHealth = hasGranary ? _granary.CurrentHealth : 0;
        int nextMaxHealth = hasGranary ? _granary.GetMaxHealthAtLevel(nextLevel) : 0;

        // Level up adds the same amount to current health as it adds to max health.
        int nextHealth = Mathf.Min(currentHealth + Mathf.Max(0, nextMaxHealth - currentMaxHealth), nextMaxHealth);

        if (_currentLevelText != null) _currentLevelText.text = $"{currentLevel}";
        if (_nextLevelText != null) _nextLevelText.text = isMax ? "MAX" : $"{nextLevel}";

        if (_currentHealthText != null) _currentHealthText.text = $"{currentHealth}/{currentMaxHealth}";
        if (_nextHealthText != null) _nextHealthText.text = isMax ? "MAX" : $"{nextHealth}/{nextMaxHealth}";

        if (_currentHealthBar != null) _currentHealthBar.SetHP(currentHealth, currentMaxHealth);
        if (_nextHealthBar != null) _nextHealthBar.SetHP(nextHealth, nextMaxHealth);

        if (_levelUpCostText != null)
            _levelUpCostText.text = isMax ? "MAX" : _granary.GetLevelUpPrice().ToString();

        if (_levelUpButton != null)
        {
            _levelUpButton.interactable = hasGranary
                && _granary.CanLevelUp
                && _currencyManager != null
                && _currencyManager.Money >= _granary.GetLevelUpPrice();
        }
    }
}