using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _typeText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _buyButton;

    [Header("Price Display")]
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(1f, 0.4f, 0.4f);

    public event Action OnBuyClicked;

    private void Awake()
    {
        if (_buyButton != null)
            _buyButton.onClick.AddListener(HandleBuyButton);
    }

    private void OnDestroy()
    {
        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(HandleBuyButton);
    }

    /// <summary>
    /// Shows one item. The cost color depends on the money, the Buy button on money and slot availability.
    /// </summary>
    public void Show(ItemDataSO data, bool canAfford, bool isAvailable)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        if (_icon != null)
        {
            _icon.sprite = data.icon;
            _icon.enabled = data.icon != null;
        }

        if (_nameText != null) _nameText.text = data.displayName;
        if (_descriptionText != null) _descriptionText.text = data.description;
        if (_typeText != null) _typeText.text = data.itemType.ToString();

        if (_costText != null)
        {
            _costText.text = $"Cost: {data.price}";
            _costText.color = canAfford ? _affordableColor : _unaffordableColor;
        }

        if (_buyButton != null)
            _buyButton.interactable = canAfford && isAvailable;
    }

    /// <summary>
    /// Empties every text, hides the icon and disables the Buy button.
    /// </summary>
    public void Clear()
    {
        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        if (_nameText != null) _nameText.text = string.Empty;
        if (_descriptionText != null) _descriptionText.text = string.Empty;
        if (_typeText != null) _typeText.text = string.Empty;
        if (_costText != null) _costText.text = string.Empty;

        if (_buyButton != null)
            _buyButton.interactable = false;
    }

    private void HandleBuyButton()
    {
        Debug.Log("[ItemDetailUI] Buy button clicked.");
        OnBuyClicked?.Invoke();
    }
}