using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemRadialSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _txtCurrentStack;

    private ItemDataSO _currentData;

    /// <summary>
    /// Sets up the radial slot with item data and current stack count.
    /// Completely hides the slot GameObject if no item is equipped.
    /// </summary>
    public void Setup(ItemDataSO data, int currentStack)
    {
        _currentData = data;

        bool hasItem = data != null && data.icon != null && currentStack > 0;

        gameObject.SetActive(hasItem);

        if (!hasItem) return;

        if (_itemIcon != null)
        {
            _itemIcon.sprite = data.icon;
            _itemIcon.gameObject.SetActive(true);
        }

        UpdateStackText(currentStack);
    }

    /// <summary>
    /// Updates the stack count text on the radial slot.
    /// </summary>
    private void UpdateStackText(int stackAmount)
    {
        if (_txtCurrentStack != null)
        {
            _txtCurrentStack.text = stackAmount > 1 ? stackAmount.ToString() : string.Empty;
            _txtCurrentStack.gameObject.SetActive(stackAmount > 1);
        }
    }
}