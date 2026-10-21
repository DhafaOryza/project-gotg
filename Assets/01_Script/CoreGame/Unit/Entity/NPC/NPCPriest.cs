using UnityEngine;

public class NPCPriest : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.HealthUI != null)
        {
            PlayerUI.Instance.HealthUI.gameObject.SetActive(true);
        }
    }
}