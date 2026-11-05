using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableCardUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _selectButton;

    private ConsumableDataSO _data;

    public event Action<ConsumableCardUI> OnSelected;

    public ConsumableDataSO Data => _data;

    private void Awake()
    {
        if (_selectButton == null)
            _selectButton = GetComponent<Button>();

        if (_selectButton != null)
            _selectButton.onClick.AddListener(HandleSelectButton);
    }

    private void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelectButton);
    }

    /// <summary>
    /// Fills the card with the icon and name of one consumable.
    /// </summary>
    public void Initialize(ConsumableDataSO data)
    {
        _data = data;

        if (_icon != null)
        {
            _icon.sprite = data != null ? data.icon : null;
            _icon.enabled = _icon.sprite != null;
        }

        if (_nameText != null)
            _nameText.text = data != null ? data.displayName : string.Empty;
    }

    private void HandleSelectButton()
    {
        OnSelected?.Invoke(this);
    }
}