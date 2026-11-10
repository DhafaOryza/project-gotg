using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCardUI : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private Image cardIcon;
    [SerializeField] private TextMeshProUGUI txtTitle; // Align Kiri Bawah
    [SerializeField] private TextMeshProUGUI txtValue; // Align Kanan Bawah

    public void Setup(Sprite icon, string title, string value)
    {
        if (cardIcon != null)
        {
            cardIcon.sprite = icon;
            cardIcon.enabled = icon != null;
        }

        if (txtTitle != null) txtTitle.text = title;
        if (txtValue != null) txtValue.text = value;
    }
}