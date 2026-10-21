using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNodeSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _slotButton;
    [SerializeField] private GameObject _lockOverlay;

    /// <summary>
    /// Setups the visual components of the skill slot.
    /// </summary>
    public void SetupSlot(Sprite icon, int cost, bool isLocked)
    {
        if (_iconImage != null && icon != null)
        {
            _iconImage.sprite = icon;
        }

        if (_costText != null)
        {
            _costText.text = cost.ToString();
        }

        SetLockedState(isLocked);
    }

    /// <summary>
    /// Updates the locked visual state and button interactability.
    /// </summary>
    public void SetLockedState(bool isLocked)
    {
        if (_lockOverlay != null)
        {
            _lockOverlay.SetActive(isLocked);
        }

        if (_slotButton != null)
        {
            _slotButton.interactable = !isLocked;
        }
    }
}