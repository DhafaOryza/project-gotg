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
            }
            else
            {
                Debug.Log("Deck sudah Penuh !");
                card.ResetToOriginalParent();
            }
        }
    }
}