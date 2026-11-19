using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsDetailPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _statIcon;
    [SerializeField] private TextMeshProUGUI _statName;
    [SerializeField] private TextMeshProUGUI _statDescription;
    [SerializeField] private TextMeshProUGUI _statLevel;
    [SerializeField] private TextMeshProUGUI _statCurrentValue;
    [SerializeField] private TextMeshProUGUI _statNextValue;
    [SerializeField] private TextMeshProUGUI _statUpgradeCost;
    [SerializeField] private Button _btnUpgrade;

    private AdditionalStatsSO currentStat;
    private CurrencyManager currencyManager;

    public event Action<AdditionalStatsSO> OnStatUpgraded;

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf() => gameObject.SetActive(false);

    private void OnEnable()
    {
        if (_btnUpgrade != null)
            _btnUpgrade.onClick.AddListener(HandleUpgradeButton);

        SubscribeCurrency();
    }

    private void OnDisable()
    {
        if (_btnUpgrade != null)
            _btnUpgrade.onClick.RemoveListener(HandleUpgradeButton);

        if (currencyManager != null)
        {
            currencyManager.OnChange -= HandleCurrencyChanged;
            currencyManager = null;
        }

    }

    public void Initialize(AdditionalStatsSO stat)
    {
        currentStat = stat;
        if (isActiveAndEnabled)
            SubscribeCurrency();
        UpdateUI();
    }

    private void SubscribeCurrency()
    {
        var manager = GameManager.Instance?.currencyManager;
        if (currencyManager == manager) return;

        if (currencyManager != null)
            currencyManager.OnChange -= HandleCurrencyChanged;

        currencyManager = manager;
        if (currencyManager != null)
            currencyManager.OnChange += HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentStat == null)
        {
            if (_statIcon != null) _statIcon.sprite = null;
            if (_statName != null) _statName.text = string.Empty;
            if (_statDescription != null) _statDescription.text = string.Empty;
            if (_statLevel != null) _statLevel.text = string.Empty;
            if (_statCurrentValue != null) _statCurrentValue.text = string.Empty;
            if (_statNextValue != null) _statNextValue.text = string.Empty;
            if (_statUpgradeCost != null) _statUpgradeCost.text = string.Empty;
            return;
        }

        if (_statIcon != null) _statIcon.sprite = currentStat.Icon;
        if (_statName != null) _statName.text = currentStat.Type.ToString();
        if (_statDescription != null) _statDescription.text = currentStat.Description;
        if (_statLevel != null) _statLevel.text = $"Level: {currentStat.Level}/{currentStat.MaxLevel}";
        if (_statCurrentValue != null) _statCurrentValue.text = $"Current: {currentStat.GetCalculatedValue()}";
        if (_statNextValue != null)
        {
            _statNextValue.text = currentStat.IsMaxLevel
                ? "Next: MAX"
                : $"Next: {currentStat.GetCalculatedValueAtLevel(currentStat.Level + 1)}";
        }

        int price = currentStat.GetUpgradePrice();
        if (_statUpgradeCost != null)
            _statUpgradeCost.text = currentStat.IsMaxLevel ? "Cost: MAX" : $"Cost: {price}";

        if (_btnUpgrade != null)
            _btnUpgrade.interactable = !currentStat.IsMaxLevel
                && currencyManager != null
                && currencyManager.Money >= price;
    }

    private void HandleUpgradeButton()
    {
        if (currentStat == null || currentStat.IsMaxLevel) return;

        var player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
        if (player == null || currencyManager == null) return;

        var playerStats = player.AdditionalStats;
        if (playerStats == null) return;

        bool belongsToPlayer = false;
        foreach (var stat in playerStats)
        {
            if (stat == currentStat)
            {
                belongsToPlayer = true;
                break;
            }
        }

        if (!belongsToPlayer) return;

        int price = currentStat.GetUpgradePrice();
        if (!currencyManager.SpendMoney(price)) return;

        if (!player.LevelUpAdditionalStat(currentStat))
        {
            currencyManager.AddMoney(price);
            UpdateUI();
            return;
        }

        UpdateUI();
        OnStatUpgraded?.Invoke(currentStat);
    }
}