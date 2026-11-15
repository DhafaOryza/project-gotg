using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsDetailUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _statIcon;
    [SerializeField] private TextMeshProUGUI _statName;
    [SerializeField] private TextMeshProUGUI _statDescription;
    [SerializeField] private TextMeshProUGUI _statLevel;
    [SerializeField] private TextMeshProUGUI _currentValue;
    [SerializeField] private TextMeshProUGUI _nextLevelValue;
    [SerializeField] private TextMeshProUGUI _upgradeCost;

    public void Show(AdditionalStatsSO stats)
    {
        if (stats == null)
        {
            Clear();
            return;
        }

        if (_statIcon != null)
        {
            _statIcon.sprite = stats.Icon;
            _statIcon.enabled = stats.Icon != null;
        }

        if (_statName != null) _statName.text = stats.Type.ToString();
        if (_statDescription != null) _statDescription.text = stats.Description;
        if (_statLevel != null) _statLevel.text = $"Lv. {stats.Level}/{stats.MaxLevel}";
        if (_currentValue != null) _currentValue.text = $"Current Value: {FormatValue(stats.GetCalculatedValue())}";

        bool isMaxLevel = stats.IsMaxLevel;
        if (_nextLevelValue != null)
        {
            _nextLevelValue.text = isMaxLevel
                ? "Next Level: MAX"
                : $"Next Level: {FormatValue(stats.GetCalculatedValueAtLevel(stats.Level + 1))}";
        }

        if (_upgradeCost != null)
            _upgradeCost.text = isMaxLevel ? "Upgrade Cost: MAX" : $"Upgrade Cost: {stats.GetUpgradePrice()}";
    }

    public void Clear()
    {
        if (_statIcon != null)
        {
            _statIcon.sprite = null;
            _statIcon.enabled = false;
        }

        if (_statName != null) _statName.text = string.Empty;
        if (_statDescription != null) _statDescription.text = string.Empty;
        if (_statLevel != null) _statLevel.text = string.Empty;
        if (_currentValue != null) _currentValue.text = string.Empty;
        if (_nextLevelValue != null) _nextLevelValue.text = string.Empty;
        if (_upgradeCost != null) _upgradeCost.text = string.Empty;
    }

    private static string FormatValue(float value)
    {
        return value.ToString("0.##");
    }
}