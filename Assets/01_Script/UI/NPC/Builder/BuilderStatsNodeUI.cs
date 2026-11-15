using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderStatsNodeUI : MonoBehaviour
{
    [SerializeField] private Image _statsIcon;
    [SerializeField] private TextMeshProUGUI _statsType;
    [SerializeField] private TextMeshProUGUI _statsDesc;
    [SerializeField] private TextMeshProUGUI _statsLevel;
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TextMeshProUGUI _upgradePrice;

    [Header("Price Display")]
    [SerializeField] private string _maxLevelText = "MAX";
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(1f, 0.4f, 0.4f);

    private AdditionalStatsSO _stats;
    private BuildingBase _granary;
    private CurrencyManager _currencyManager;

    public event Action<BuilderStatsNodeUI> OnUpgraded;
    public event Action<BuilderStatsNodeUI> OnSelected;

    public AdditionalStatsSO Stats => _stats;

    private void Awake()
    {
        if (_selectButton == null)
            _selectButton = GetComponent<Button>();

        // Listeners are added once here, not in Initialize, so they never stack up.
        if (_selectButton != null)
            _selectButton.onClick.AddListener(HandleSelectButton);

        if (_upgradeButton != null)
            _upgradeButton.onClick.AddListener(HandleUpgradeButton);
    }

    private void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelectButton);

        if (_upgradeButton != null)
            _upgradeButton.onClick.RemoveListener(HandleUpgradeButton);

        UnsubscribeCurrency();
    }

    /// <summary>
    /// Binds this node to one stat of the given granary.
    /// </summary>
    public void Initialize(AdditionalStatsSO stats, BuildingBase granary)
    {
        _stats = stats;
        _granary = granary;

        // The CurrencyManager is fetched here because it does not exist yet in Awake of a new node.
        UnsubscribeCurrency();
        _currencyManager = GameManager.Instance != null ? GameManager.Instance.currencyManager : null;
        if (_currencyManager != null)
            _currencyManager.OnChange += HandleCurrencyChanged;

        Refresh();
    }

    /// <summary>
    /// Updates every text and the button state.
    /// </summary>
    public void Refresh()
    {
        if (_stats == null)
        {
            if (_statsIcon != null) _statsIcon.sprite = null;
            if (_statsType != null) _statsType.text = string.Empty;
            if (_statsDesc != null) _statsDesc.text = string.Empty;
            if (_statsLevel != null) _statsLevel.text = string.Empty;
            if (_upgradePrice != null) _upgradePrice.text = string.Empty;
            if (_upgradeButton != null) _upgradeButton.interactable = false;
            return;
        }

        if (_statsIcon != null) _statsIcon.sprite = _stats.Icon;
        if (_statsType != null) _statsType.text = _stats.Type.ToString();
        if (_statsDesc != null) _statsDesc.text = _stats.Description;
        if (_statsLevel != null) _statsLevel.text = $"Lv. {_stats.Level}/{_stats.MaxLevel}";

        bool canUpgrade = CanUpgrade();

        if (_upgradePrice != null)
        {
            _upgradePrice.text = _stats.IsMaxLevel ? _maxLevelText : _stats.GetUpgradePrice().ToString();
            _upgradePrice.color = _stats.IsMaxLevel || canUpgrade ? _affordableColor : _unaffordableColor;
        }

        if (_upgradeButton != null)
            _upgradeButton.interactable = canUpgrade;
    }

    private bool CanUpgrade()
    {
        if (_stats == null || _stats.IsMaxLevel || _currencyManager == null) return false;

        return _currencyManager.Money >= _stats.GetUpgradePrice();
    }

    private void HandleUpgradeButton()
    {
        OnSelected?.Invoke(this);

        // Validate again on click: the money may have changed since the last refresh.
        if (!CanUpgrade() || _granary == null)
        {
            Refresh();
            return;
        }

        int price = _stats.GetUpgradePrice();
        if (!_currencyManager.SpendMoney(price))
        {
            Refresh();
            return;
        }

        if (!_granary.LevelUpAdditionalStat(_stats))
        {
            _currencyManager.AddMoney(price);
            Refresh();
            return;
        }

        Refresh();
        OnUpgraded?.Invoke(this);
    }

    private void HandleSelectButton()
    {
        OnSelected?.Invoke(this);
    }

    private void HandleCurrencyChanged()
    {
        Refresh();
    }

    private void UnsubscribeCurrency()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= HandleCurrencyChanged;
    }
}