using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    [Header("References")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _txtCurrentStack;
    [SerializeField] private TextMeshProUGUI _txtSlotLabel;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _none;
    [SerializeField] private Button _btnRemove;

    private ItemDataSO _currentData;
    private int _loadoutSlotIndex = -1;

    private void Awake()
    {
        if (_btnRemove != null)
            _btnRemove.onClick.AddListener(RemoveFromLoadout);
    }

    private void OnDestroy()
    {
        if (_btnRemove != null)
            _btnRemove.onClick.RemoveListener(RemoveFromLoadout);
    }

    /// <summary>
    /// Sets up the slot for general inventory display.
    /// </summary>
    public void Setup(ItemDataSO data, int currentStack)
    {
        _loadoutSlotIndex = -1;
        _currentData = data;

        if (data == null || currentStack <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (_itemIcon != null)
            _itemIcon.sprite = data.icon;

        if (_info != null) _info.SetActive(true);
        if (_none != null) _none.SetActive(false);

        UpdateStackText(currentStack);
    }

    /// <summary>
    /// Sets up the loadout slot with item data, slot index, label, and stack amount.
    /// Handles info and none GameObject states similar to SkillSlotUI.
    /// </summary>
    public void SetupLoadoutSlot(int slotIndex, string slotLabel, ItemDataSO data, int currentStack)
    {
        _loadoutSlotIndex = slotIndex;
        _currentData = data;
        gameObject.SetActive(true);

        if (_txtSlotLabel != null)
            _txtSlotLabel.text = slotLabel;

        bool hasItem = data != null;

        if (hasItem)
        {
            if (_info != null) _info.SetActive(true);
            if (_none != null) _none.SetActive(false);

            if (_itemIcon != null)
                _itemIcon.sprite = data.icon;

            if (_itemNameText != null)
                _itemNameText.text = data.displayName;

            UpdateStackText(currentStack);
        }
        else
        {
            if (_info != null) _info.SetActive(false);
            if (_none != null) _none.SetActive(true);

            UpdateStackText(0);
        }
    }

    /// <summary>
    /// Handles item drop event onto the loadout slot.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (_loadoutSlotIndex < 0 || eventData.pointerDrag == null) return;

        var card = eventData.pointerDrag.GetComponent<ItemCardUI>();
        if (card == null || card.Data == null) return;

        var manager = GameManager.Instance?.itemManager;
        manager?.EquipItems(_loadoutSlotIndex, card.Data);
    }

    /// <summary>
    /// Removes the item from the current loadout slot.
    /// </summary>
    private void RemoveFromLoadout()
    {
        if (_loadoutSlotIndex < 0 || _currentData == null) return;

        GameManager.Instance?.itemManager?.RemoveItemsAt(_loadoutSlotIndex);
    }

    /// <summary>
    /// Updates the stack count display on the slot.
    /// </summary>
    public void UpdateStackText(int stackAmount)
    {
        if (_txtCurrentStack != null)
        {
            _txtCurrentStack.text = stackAmount > 0 ? stackAmount.ToString() : string.Empty;
            _txtCurrentStack.gameObject.SetActive(stackAmount > 0);
        }
    }
}