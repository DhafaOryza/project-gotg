using UnityEngine;

public class NPCGuard : NPCBaseEntity
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