using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderRepairDetail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _repairButton;
    [SerializeField] private TextMeshProUGUI _repairPriceText;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private SliderBar _healthBar;

    private BuildingBase _granary;
    private CurrencyManager _currencyManager;

    private void Awake()
    {
        if (_repairButton != null)
            _repairButton.onClick.AddListener(HandleRepairButton);
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
        if (_repairButton != null)
            _repairButton.onClick.RemoveListener(HandleRepairButton);
    }

    /// <summary>
    /// Sets the granary to repair and refreshes the display.
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
    /// Pays the repair price and repairs the granary. The money is refunded if it fails.
    /// </summary>
    private void HandleRepairButton()
    {
        if (_granary == null || _currencyManager == null || !_granary.CanRepair)
        {
            Refresh();
            return;
        }

        int price = _granary.RepairPrice;
        if (!_currencyManager.SpendMoney(price))
        {
            Refresh();
            return;
        }

        if (!_granary.Repair())
            _currencyManager.AddMoney(price);

        Refresh();
    }

    private void Refresh()
    {
        bool hasGranary = _granary != null;
        bool canRepair = hasGranary && _granary.CanRepair;

        int currentHealth = hasGranary ? _granary.CurrentHealth : 0;
        int maxHealth = hasGranary ? _granary.MaxHealth : 0;

        if (_healthText != null) _healthText.text = $"{currentHealth}/{maxHealth}";
        if (_healthBar != null) _healthBar.SetHP(currentHealth, maxHealth);

        if (_repairPriceText != null)
            _repairPriceText.text = canRepair ? _granary.RepairPrice.ToString() : "FULL";

        if (_repairButton != null)
        {
            _repairButton.interactable = canRepair
                && _currencyManager != null
                && _currencyManager.Money >= _granary.RepairPrice;
        }
    }
}