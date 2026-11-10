using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image ItemIcon;
    [SerializeField] private TextMeshProUGUI txtCurrentStack;
    [SerializeField] private TextMeshProUGUI txtSlotLabel;
    [SerializeField] private Button btnRemove;

    private ItemDataSO currentData;
    private int loadoutSlotIndex = -1;

    private void Awake()
    {
        if (btnRemove != null)
            btnRemove.onClick.AddListener(RemoveFromLoadout);
    }

    private void OnDestroy()
    {
        if (btnRemove != null)
            btnRemove.onClick.RemoveListener(RemoveFromLoadout);
    }

    public void Setup(ItemDataSO data, int currentStack)
    {
        loadoutSlotIndex = -1;
        currentData = data;

        if (data == null || currentStack <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (ItemIcon != null)
            ItemIcon.sprite = data.icon;

        UpdateStackText(currentStack);
    }

    public void SetupLoadoutSlot(int slotIndex, string slotLabel, ItemDataSO data, int currentStack)
    {
        loadoutSlotIndex = slotIndex;
        currentData = data;
        gameObject.SetActive(true);

        if (txtSlotLabel != null)
            txtSlotLabel.text = slotLabel;

        if (ItemIcon != null)
        {
            ItemIcon.sprite = data != null ? data.icon : null;
            ItemIcon.enabled = data != null && data.icon != null;
        }

        UpdateStackText(data != null ? currentStack : 0);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (loadoutSlotIndex < 0 || eventData.pointerDrag == null) return;

        var card = eventData.pointerDrag.GetComponent<ItemCardUI>();
        if (card == null || card.Data == null) return;

        var manager = GameManager.Instance?.itemManager;
        manager?.EquipItems(loadoutSlotIndex, card.Data);
    }

    private void RemoveFromLoadout()
    {
        if (loadoutSlotIndex < 0 || currentData == null) return;

        GameManager.Instance?.itemManager?.RemoveItemsAt(loadoutSlotIndex);
    }

    public void UpdateStackText(int stackamount)
    {
        if (txtCurrentStack != null)
        {
            txtCurrentStack.text = stackamount.ToString();
            txtCurrentStack.gameObject.SetActive(stackamount > 0);
        }
    }
}