using UnityEngine;

public class NPCGuard : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.SkillTreeUI != null)
        {
            PlayerUI.Instance.SkillTreeUI.gameObject.SetActive(true);
        }
    }
}