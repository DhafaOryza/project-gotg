using UnityEngine;

public class NPCPriest : NPCBaseEntity
{
    protected override bool WillOpenPanel() => true;

    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.HandleNpcRolePanel(_npcData, _npcMovement);
        }
        else
        {
            ResumeMovement();
        }
    }
}