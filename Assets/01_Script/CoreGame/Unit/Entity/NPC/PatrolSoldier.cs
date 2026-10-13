using UnityEngine;

public class PatrolSoldier : NPCBaseEntity
{
    [Header("Skill Tree UI")]
    private SkillTreeManagerUI skillTreeManagerUI;

    protected override void Awake()
    {
        base.Awake();
        skillTreeManagerUI = Object.FindAnyObjectByType<SkillTreeManagerUI>(FindObjectsInactive.Include);
    }
    public override void Interact(PlayerBaseEntity player)
    {
        if (ConversationUIManager.Instance != null && entityData is NPCDataSO npcData)
        {
            ConversationUIManager.Instance.StartConversation(npcData, () =>
            {
                if (skillTreeManagerUI != null)
                {
                    skillTreeManagerUI.gameObject.SetActive(true);
                    skillTreeManagerUI.RefreshSkillTree();  
                }
            });
        }
    }
}