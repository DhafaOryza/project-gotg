using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillCooldown;

    public void Initialize(SkillDataSO currentSkill)
    {
        if (skillIcon == null || skillName == null || skillCooldown == null) return;

        skillIcon.sprite = currentSkill.icon;
        skillName.text = currentSkill.displayName;
        skillCooldown.text = $"Cooldown : {currentSkill.cooldown}s";
    }
}