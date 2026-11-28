using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _slotButton;
    [SerializeField] private GameObject _purchasedOverlayObject;

    private SkillDataSO _skillData;
    private System.Action<SkillDataSO, SkillNodeSlot> _onSelectedCallback;

    public SkillDataSO SkillData => _skillData;

    /// <summary>
    /// Sets up the skill slot UI components and visual state.
    /// </summary>
    public void SetupSlot(SkillDataSO skillData, System.Action<SkillDataSO, SkillNodeSlot> onSelected)
    {
        _skillData = skillData;
        _onSelectedCallback = onSelected;

        if (_iconImage != null)
            _iconImage.sprite = skillData.icon;

        if (_nameText != null)
            _nameText.text = skillData.displayName;

        if (_descriptionText != null)
            _descriptionText.text = skillData.description;

        if (_costText != null)
            _costText.text = $"{skillData.price}";

        UpdatePurchasedVisual(skillData.isPurchased);
        RefreshButtonState();

        if (_slotButton != null)
        {
            _slotButton.onClick.RemoveAllListeners();
            _slotButton.onClick.AddListener(() => _onSelectedCallback?.Invoke(_skillData, this));
        }
    }

    /// <summary>
    /// Refreshes the button interactability and purchased overlay based on current state.
    /// Call this after a purchase or currency change.
    /// </summary>
    public void Refresh()
    {
        if (_skillData == null) return;

        // UpdateLockVisual(_skillData.locked);
        UpdatePurchasedVisual(_skillData.isPurchased);
        RefreshButtonState();
    }

    /// <summary>Shows or hides the purchased overlay.</summary>
    public void UpdatePurchasedVisual(bool isPurchased)
    {
        if (_purchasedOverlayObject != null)
            _purchasedOverlayObject.SetActive(!isPurchased);
    }

    /// <summary>
    /// Disables the button when the skill is already purchased, locked, or the player cannot afford it.
    /// </summary>
    private void RefreshButtonState()
    {
        if (_slotButton == null || _skillData == null) return;

        var currency = GameManager.Instance?.currencyManager;
        bool canAfford = currency != null && currency.Money >= _skillData.price;

        _slotButton.interactable = !_skillData.isPurchased
                                   && !_skillData.locked
                                   && canAfford;
    }
}