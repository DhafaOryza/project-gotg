using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI txtKey;

    [SerializeField] private GameObject none;

    public void Setup(string keyBind, SkillDataSO currentSkill = null)
    {
        txtKey.text = keyBind;

        if (currentSkill == null)
        {
            skillIcon.gameObject.SetActive(false);
            none.SetActive(true);
            return;
        }

        skillIcon.sprite = currentSkill.icon;

        skillIcon.gameObject.SetActive(true);
        none.SetActive(false);
    }
}