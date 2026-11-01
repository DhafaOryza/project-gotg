using UnityEngine;

public class NPCPriest : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.StatsStore != null)
        {
            PlayerUI.Instance.StatsStore.gameObject.SetActive(true);
        }
    }
}