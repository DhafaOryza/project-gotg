using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour
{
    public List<ItemDataSO> chosenItems = new List<ItemDataSO>();
    public const int MaxSlots = 4;

    [SerializeField] private List<ItemRuntimeData> runtimeData = new List<ItemRuntimeData>();
    public event Action OnChosenItemsChanged;
    public int EquippedCount
    {
        get
        {
            int Count = 0;
            foreach ( var item in chosenItems)
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

    public void InitializeRuntime()
    {
        runtimeData.Clear();
        for (int i = 0; i < MaxSlots; i++)
        {
            if (i < chosenItems.Count && chosenItems[i] != null)
            {
                var ItemSO = chosenItems[i];
                runtimeData.Add(new ItemRuntimeData(ItemSO, ItemSO.maxStack));
            }
            else
            {
                runtimeData.Add(null);
            }
        }
    }
    
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
        if (isSucces)
        {
            runtime.ConsumeOne();

            if (!runtime.HasStock)
            {
                chosenItems[slotIndex] = null;
                runtimeData[slotIndex] = null;
            }

            NotifyChanged();
            return true;
        }

        return false;
    }
    public bool EquipItems(int slotIndex, ItemDataSO dataSO)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return false;

        EnsureSlotCount();
        while (runtimeData.Count < MaxSlots)
            runtimeData.Add(null);

        int existing = chosenItems.IndexOf(dataSO);
        if (existing == slotIndex)
            return true;

        if (existing < 0 && chosenItems[slotIndex] != null)
            return false;

        ItemDataSO displaced = chosenItems[slotIndex];
        ItemRuntimeData displacedRuntime = runtimeData[slotIndex];
        ItemRuntimeData incomingRuntime = existing >= 0
            ? runtimeData[existing]
            : new ItemRuntimeData(dataSO, dataSO.maxStack);

        chosenItems[slotIndex] = dataSO;
        runtimeData[slotIndex] = incomingRuntime;

        if (existing >= 0)
        {
            chosenItems[existing] = displaced;
            runtimeData[existing] = displacedRuntime;
        }

        NotifyChanged();
        return true;
    }

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
    /// True if the item is not equipped yet and a free slot is available.
    /// </summary>
    public bool CanAddItem(ItemDataSO dataSO)
    {
        if (dataSO == null) return false;

        EnsureSlotCount();
        if (chosenItems.Contains(dataSO)) return false;

        return FindEmptySlot() >= 0;
    }

    /// <summary>
    /// Puts a item with a full stack into the first free slot. Other slots keep their current stock.
    /// </summary>
    public bool TryAddItem(ItemDataSO dataSO)
    {
        if (!CanAddItem(dataSO)) return false;

        int slotIndex = FindEmptySlot();
        chosenItems[slotIndex] = dataSO;

        // Unlike EquipItems, the runtime data is NOT rebuilt, so other slots keep their stock.
        while (runtimeData.Count < MaxSlots)
            runtimeData.Add(null);

        runtimeData[slotIndex] = new ItemRuntimeData(dataSO, dataSO.maxStack);

        NotifyChanged();
        return true;
    }

    public void ClearAll()
    {
        bool isChanged = false;
        for (int i = 0; i < chosenItems.Count; i++)
        {
            if (chosenItems[i] == null) return;
            chosenItems[i] = null;
            isChanged = true;
        }

        if (isChanged) NotifyChanged();
    }

    public void NotifyChanged() => OnChosenItemsChanged?.Invoke();
    public void EnsureSlotCount()
    {
        while (chosenItems.Count < MaxSlots)
            chosenItems.Add(null);

        if (chosenItems.Count > MaxSlots)
            chosenItems.RemoveRange(MaxSlots, chosenItems.Count - MaxSlots);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < MaxSlots; i++)
            if (chosenItems[i] == null) return i;

        return -1;
    }
}