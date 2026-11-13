using UnityEngine;

public class ItemLoadoutPanel : MonoBehaviour
{
    [Header("References")]
    [Header("Item Card")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ItemCardUI itemCardPrefab;

    [Header("Item Slot")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;


    private void OnEnable()
    {
        SubscribeToItemManager();
        Initialize();
    }

    private void OnDisable()
    {
        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager != null)
            itemManager.OnChosenItemsChanged -= GenerateItemSlot;
    }

    public void Initialize()
    {
        GenerateItemCard();
        GenerateItemSlot();
    }

    #region Helper

    private void GenerateItemCard()
    {
        if (itemContainer == null || itemCardPrefab == null) return;

        ClearContainer(itemContainer);

        var items = GameSessionData.GetOrCreate()?.items;
        if (items == null) return;

        foreach (var item in items)
        {
            if (item == null || item.stack <= 0) continue;

            ItemCardUI card = Instantiate(itemCardPrefab, itemContainer);
            card.Initialize(item);
        }
    }

    private void GenerateItemSlot()
    {
        if (itemSlotContainer == null || itemSlotPrefab == null) return;

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

        ClearContainer(itemSlotContainer);

        for (int i = 0; i < ItemManager.MaxSlots; i++)
        {
            var runtime = itemManager.GetRuntimeSlot(i);
            ItemDataSO data = runtime != null && runtime.HasStock ? runtime.data : null;
            int stack = data != null ? runtime.currentStack : 0;

            ItemSlotUI slot = Instantiate(itemSlotPrefab, itemSlotContainer);
            slot.SetupLoadoutSlot(i, $"Item {i + 1}", data, stack);
        }
    }

    private void SubscribeToItemManager()
    {
        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

        itemManager.OnChosenItemsChanged -= GenerateItemSlot;
        itemManager.OnChosenItemsChanged += GenerateItemSlot;
    }

    private static void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    #endregion
}
