using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI txtKey;

    public void Setup(Sprite icon, string textKey)
    {
        skillIcon.sprite = icon;
        txtKey.text = textKey;
    }
}