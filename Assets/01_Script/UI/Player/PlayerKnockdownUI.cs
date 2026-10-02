using UnityEngine;
using TMPro;
public class PlayerKnockdownUI : MonoBehaviour
{
    [SerializeField] private GameObject KnockdownPanel;
    [SerializeField] private TMP_Text timerText;
    private PlayerBaseEntity player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerBaseEntity>();

        if (KnockdownPanel != null)
            KnockdownPanel.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerBaseEntity>();
            return;
        }

        if (player.IsKnockDown)
        {
            if (KnockdownPanel != null && !KnockdownPanel.activeSelf)
                KnockdownPanel.SetActive(true);

            if (timerText != null)
            {
                int remainingCooldown = Mathf.CeilToInt(player.CurrentReviveTimer);
                timerText.text = $"Kamu akan di revive setelah {remainingCooldown} detik";
            }
        }
        else
        {
            if (KnockdownPanel != null && KnockdownPanel.activeSelf)
                KnockdownPanel.SetActive(false);
        }
    }
}