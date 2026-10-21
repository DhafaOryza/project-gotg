using UnityEngine;

public class NPCMerchant : NPCBaseEntity
{
    protected override void OnConversationEnded()
    {
        base.OnConversationEnded();

        if (PlayerUI.Instance != null && PlayerUI.Instance.ShopUI != null)
        {
            PlayerUI.Instance.ShopUI.gameObject.SetActive(true);
        }
    }
}