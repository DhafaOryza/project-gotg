using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillDeckUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private int maxSizeDeck;
    
    public void OnDrop(PointerEventData eventData)
    {
        SkillSelectorCardUI card = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<SkillSelectorCardUI>(): null;
        if (card != null)
        {
            if (transform.childCount < maxSizeDeck)
            {
                card.SetNewParent(transform);
                SyncSkillToGameSession();
            }
            else
            {
                Debug.Log("Deck sudah Penuh !");
                card.ResetToOriginalParent();
            }
        }
    }

    public void SyncSkillToGameSession()
    {
        List<SkillDataSO> currentDeckSkill = new List<SkillDataSO>();

        foreach (Transform child in transform)
        {
            SkillSelectorCardUI card = child.GetComponent<SkillSelectorCardUI>();
            if (card != null && card.SkillData != null)
                currentDeckSkill.Add(card.SkillData);
        }

        if (GameSessionData.Instance != null)
        {
            GameSessionData.Instance.SetSkillsData(currentDeckSkill);
        }
    }
}