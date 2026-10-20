using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI keyBind;
    [SerializeField] private GameObject none;

    private SkillDataSO skillData;

    public void Initialize(string setKeyBind, SkillDataSO currentData = null)
    {
        skillData = currentData;
        keyBind.text = setKeyBind;

        if (currentData != null)
        {
            skillIcon.sprite = skillData.icon;
            skillName.text = skillData.displayName;

            none.SetActive(false);
        }
        else
        {
            none.SetActive(true);
        }
    }
}