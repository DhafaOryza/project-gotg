using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour
{
    // Loadout: the items equipped in the slots. The items themselves are stored in the inventory.
    public List<ItemDataSO> chosenItems = new List<ItemDataSO>();
    public const int MaxSlots = 4;

    // Every item the player owns. Shown as cards in the preparation panel.
    [SerializeField] private List<ItemRuntimeData> _inventory = new List<ItemRuntimeData>();

    // Runtime data of the loadout slots, same order as chosenItems.
    [SerializeField] private List<ItemRuntimeData> runtimeData = new List<ItemRuntimeData>();

    public event Action OnChosenItemsChanged;

    public IReadOnlyList<ItemRuntimeData> Inventory => _inventory;

    public int EquippedCount
    {
        get
        {
            int Count = 0;
            foreach (var item in chosenItems)
                if (item != null) Count++;

            return Count;
        }
    }

    public bool IsFull => FindEmptySlot() < 0;

    private void Awake()
    {
        EnsureSlotCount();
        InitializeRuntime();
    }

    private void OnValidate()
    {
        EnsureSlotCount();
    }

    /// <summary>
    /// Rebuilds the loadout runtime data. Equipped items that are not in the inventory yet are added to it,
    /// so an item set in "Chosen Items" for testing is owned automatically.
    /// </summary>
    public void InitializeRuntime()
    {
        EnsureSlotCount();
        _inventory.RemoveAll(owned => owned == null || owned.data == null || !owned.HasStock);

        runtimeData.Clear();
        for (int i = 0; i < MaxSlots; i++)
        {
            ItemDataSO item = chosenItems[i];
            if (item == null)
            {
                runtimeData.Add(null);
                continue;
            }

            ItemRuntimeData owned = GetOwnedRuntime(item);
            if (owned == null)
            {
                owned = new ItemRuntimeData(item, item.maxStack);
                _inventory.Add(owned);
            }

            // The slot uses the SAME object as the inventory, so using an item also lowers the owned stock.
            runtimeData.Add(owned);
        }
    }

    #region Inventory

    /// <summary>
    /// Returns the runtime data of an owned item, or null if the player does not own it.
    /// </summary>
    public ItemRuntimeData GetOwnedRuntime(ItemDataSO item)
    {
        if (item == null) return null;

        foreach (var owned in _inventory)
        {
            if (owned != null && owned.data == item)
                return owned;
        }

        return null;
    }

    public bool IsOwned(ItemDataSO item) => GetOwnedRuntime(item) != null;

    public bool IsEquipped(ItemDataSO item) => item != null && chosenItems.Contains(item);

    /// <summary>
    /// True if the item exists and the player does not own it yet.
    /// </summary>
    public bool CanAddItem(ItemDataSO dataSO)
    {
        return dataSO != null && !IsOwned(dataSO);
    }

    /// <summary>
    /// Puts a new item with a full stack into the inventory. It is NOT equipped, the player does that in the preparation panel.
    /// </summary>
    public bool TryAddItem(ItemDataSO dataSO)
    {
        if (!CanAddItem(dataSO)) return false;

        _inventory.Add(new ItemRuntimeData(dataSO, dataSO.maxStack));

        NotifyChanged();
        return true;
    }

    #endregion

    #region Loadout

    public ItemRuntimeData GetRuntimeSlot(int index)
    {
        if (index >= 0 && index < runtimeData.Count)
            return runtimeData[index];

        return null;
    }

    public ItemDataSO GetItem(int index) => index >= 0 && index < chosenItems.Count ? chosenItems[index] : null;

    public bool UseItem(int slotIndex, PlayerBaseEntity user)
    {
        var runtime = GetRuntimeSlot(slotIndex);
        if (runtime == null || runtime.data == null || !runtime.HasStock)
            return false;

        bool isSucces = runtime.data.Use(user);
        if (!isSucces) return false;

        runtime.ConsumeOne();

        // An item with no stock left is gone for good: it leaves the slot and the inventory.
        if (!runtime.HasStock)
        {
            chosenItems[slotIndex] = null;
            runtimeData[slotIndex] = null;
            _inventory.Remove(runtime);
        }

        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Equips an owned item into a slot. An item that is already in another slot swaps places with it.
    /// An item that was in the target slot is only unequipped, it stays in the inventory.
    /// </summary>
    public bool EquipItems(int slotIndex, ItemDataSO dataSO)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots || dataSO == null) return false;

        EnsureSlotCount();
        EnsureRuntimeCount();

        ItemRuntimeData owned = GetOwnedRuntime(dataSO);
        if (owned == null) return false;

        int existing = chosenItems.IndexOf(dataSO);
        if (existing == slotIndex)
            return true;

        ItemDataSO displaced = chosenItems[slotIndex];
        ItemRuntimeData displacedRuntime = runtimeData[slotIndex];

        chosenItems[slotIndex] = dataSO;
        runtimeData[slotIndex] = owned;

        if (existing >= 0)
        {
            chosenItems[existing] = displaced;
            runtimeData[existing] = displacedRuntime;
        }

        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Unequips the item in a slot. The item stays in the inventory and returns to the card list.
    /// </summary>
    public bool RemoveItemsAt(int slotindex)
    {
        if (slotindex < 0 || slotindex >= chosenItems.Count) return false;
        if (chosenItems[slotindex] == null) return false;

        chosenItems[slotindex] = null;
        if (slotindex < runtimeData.Count)
            runtimeData[slotindex] = null;

        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Removes everything: all slots and the whole inventory.
    /// </summary>
    public void ClearAll()
    {
        bool isChanged = _inventory.Count > 0;
        _inventory.Clear();

        for (int i = 0; i < chosenItems.Count; i++)
        {
            if (chosenItems[i] == null) continue;

            chosenItems[i] = null;
            isChanged = true;
        }

        for (int i = 0; i < runtimeData.Count; i++)
            runtimeData[i] = null;

        if (isChanged) NotifyChanged();
    }

    #endregion

    public void NotifyChanged() => OnChosenItemsChanged?.Invoke();

    public void EnsureSlotCount()
    {
        while (chosenItems.Count < MaxSlots)
            chosenItems.Add(null);

        if (chosenItems.Count > MaxSlots)
            chosenItems.RemoveRange(MaxSlots, chosenItems.Count - MaxSlots);
    }

    private void EnsureRuntimeCount()
    {
        while (runtimeData.Count < MaxSlots)
            runtimeData.Add(null);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < MaxSlots; i++)
            if (chosenItems[i] == null) return i;

        return -1;
    }
}