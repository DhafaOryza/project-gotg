using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SkillNodeSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _slotButton;
    [SerializeField] private Image _selectedBorder;
    [SerializeField] private GameObject _lockOverlayObject;

    private SkillDataSO _skillData;
    private System.Action<SkillDataSO, SkillNodeSlot> _onSelectedCallback;

    // Public properties using lambda syntax
    public SkillDataSO SkillData => _skillData;
    public GameObject LockOverlayObject => _lockOverlayObject;

    /// <summary>
    /// Sets up the skill slot UI components and lock visual state.
    /// </summary>
    public void SetupSlot(SkillDataSO skillData, System.Action<SkillDataSO, SkillNodeSlot> onSelected)
    {
        _skillData = skillData;
        _onSelectedCallback = onSelected;

        if (_iconImage != null) 
            _iconImage.sprite = skillData.icon;

        if (_nameText != null) 
            _nameText.text = skillData.displayName;

        if (_nameText != null) 
            _nameText.text = skillData.description;

        if (_costText != null) 
            _costText.text = skillData.baseValue.ToString();

        // Update lock visual overlay
        UpdateLockVisual(skillData.locked);

        if (_slotButton != null)
        {
            _slotButton.onClick.RemoveAllListeners();
            _slotButton.onClick.AddListener(() => _onSelectedCallback?.Invoke(_skillData, this));
        }
    }

    /// <summary>
    /// Updates the visibility of the lock overlay based on lock status.
    /// </summary>
    public void UpdateLockVisual(bool isLocked)
    {
        if (_lockOverlayObject != null)
        {
            _lockOverlayObject.SetActive(isLocked);
        }
    }
}