using UnityEngine;

public class NPCGranary : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.BuilderPanel != null)
        {
            PlayerUI.Instance.BuilderPanel.gameObject.SetActive(true);
        }
    }
}