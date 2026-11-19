using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemLoadoutPanel : MonoBehaviour
{
    [Header("References")]
    [Header("Item Card")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ItemCardUI itemCardPrefab;

    [Header("Item Slot")]
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;

    private readonly List<ItemCardUI> _spawnedCards = new List<ItemCardUI>();

    private void OnEnable()
    {
        SubscribeToItemManager();
        Initialize();
    }

    private void OnDisable()
    {
        UnsubscribeFromItemManager();
        ClearCards();
    }

    public void Initialize()
    {
        GenerateItemCard();
        GenerateItemSlot();
    }

    private void RefreshAll()
    {
        GenerateItemCard();
        GenerateItemSlot();
    }

    #region Helper

    private void GenerateItemCard()
    {
        if (itemContainer == null || itemCardPrefab == null) return;

        ClearCards();

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

        foreach (var owned in itemManager.Inventory)
        {
            if (owned == null || owned.data == null || !owned.HasStock) 
                continue;

            // Sembunyikan kartu jika sudah terpasang di loadout
            if (itemManager.IsEquipped(owned.data)) 
                continue;

            ItemCardUI card = Instantiate(itemCardPrefab, itemContainer);
            card.Initialize(owned.data);
            card.SetDraggable(true);

            card.OnSelected += HandleCardClicked;
            _spawnedCards.Add(card);
        }

        itemContainer.GetComponent<ContainerAnimator>()?.Refresh(false);
    }

    private void HandleCardClicked(ItemCardUI card)
    {
        if (card == null || card.Data == null) return;

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

        // Cari slot kosong pertama
        int emptySlotIndex = -1;
        for (int i = 0; i < ItemManager.MaxSlots; i++)
        {
            if (itemManager.GetItem(i) == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex != -1)
        {
            // 1. Simpan index kartu sebelum kartu dipindahkan dan dihancurkan
            int submittedIndex = card.transform.GetSiblingIndex();

            // 2. Pasang item ke slot
            itemManager.EquipItems(emptySlotIndex, card.Data);

            // 3. Pindahkan fokus kursor navigasi ke kartu berikutnya (sama seperti SkillCard)
            FocusNextItemCard(submittedIndex);
        }
        else
        {
            Debug.LogWarning("[ItemLoadoutPanel] Semua slot loadout sudah penuh!");
        }
    }

    /// <summary>
    /// Mengalihkan fokus controller ke kartu berikutnya agar navigasi tidak hilang
    /// </summary>
    public void FocusNextItemCard(int targetIndex)
    {
        StartCoroutine(FocusNextCardRoutine(targetIndex));
    }

    private IEnumerator FocusNextCardRoutine(int targetIndex)
    {
        // Tunggu 1 frame agar kartu lama selesai dihancurkan dan kartu baru selesai di-spawn
        yield return null;

        // Jika masih ada sisa kartu di dalam kontainer:
        if (_spawnedCards.Count > 0)
        {
            // Pilih kartu di index yang sama (yang otomatis bergeser menggantikan kartu lama)
            // Jika kartu terakhir yang ditekan, clamp ke kartu paling akhir
            int nextIndex = Mathf.Clamp(targetIndex, 0, _spawnedCards.Count - 1);
            Selectable nextCard = _spawnedCards[nextIndex].GetComponent<Selectable>();

            if (nextCard != null && nextCard.isActiveAndEnabled && nextCard.IsInteractable())
            {
                PlayerUI.Instance?.SetSelectedObject(nextCard);
                yield break;
            }
        }

        // Jika semua kartu sudah habis (semua item sudah dipasang ke slot):
        // Pindahkan fokus ke slot item pertama
        if (itemSlotContainer != null && itemSlotContainer.childCount > 0)
        {
            Selectable firstSlot = itemSlotContainer.GetChild(0).GetComponentInChildren<Selectable>();
            if (firstSlot != null && firstSlot.isActiveAndEnabled && firstSlot.IsInteractable())
            {
                PlayerUI.Instance?.SetSelectedObject(firstSlot);
            }
        }
    }

    private void GenerateItemSlot()
    {
        if (itemSlotContainer == null || itemSlotPrefab == null) return;

        ClearContainer(itemSlotContainer);

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

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

        itemManager.OnChosenItemsChanged -= RefreshAll;
        itemManager.OnChosenItemsChanged += RefreshAll;
    }

    private void UnsubscribeFromItemManager()
    {
        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null) return;

        itemManager.OnChosenItemsChanged -= RefreshAll;
    }

    private void ClearCards()
    {
        foreach (var card in _spawnedCards)
        {
            if (card != null)
                card.OnSelected -= HandleCardClicked;
        }
        _spawnedCards.Clear();

        if (itemContainer != null)
            ClearContainer(itemContainer);
    }

    private static void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    #endregion
}