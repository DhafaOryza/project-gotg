using UnityEngine;

public class NPCGranary : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.GranaryUI != null)
        {
            PlayerUI.Instance.GranaryUI.gameObject.SetActive(true);
        }
    }
}