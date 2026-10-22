using UnityEngine;
using System.Collections.Generic;
using System;

public class ConsumableAndItemManager : MonoBehaviour
{
    public List<ConsumableDataSO> chosenConsumables = new List<ConsumableDataSO>();
    public const int MaxSlots = 4;

    [SerializeField] private List<ConsumableRuntimeData> runtimeData = new List<ConsumableRuntimeData>();
    public event Action OnChosenConsumablesChanged;
    public int EquippedCount
    {
        get
        {
            int Count = 0;
            foreach ( var item in chosenConsumables)
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
            if (i < chosenConsumables.Count && chosenConsumables[i] != null)
            {
                var ItemSO = chosenConsumables[i];
                runtimeData.Add(new ConsumableRuntimeData(ItemSO, ItemSO.maxStack));
            }
            else
            {
                runtimeData.Add(null);
            }
        }
    }
    
    public ConsumableRuntimeData GetRuntimeSlot(int index)
    {
        if (index >= 0 && index < runtimeData.Count)
            return runtimeData[index];

        return null;
    }
    public ConsumableDataSO GetConsumable(int index) => index >= 0 && index < chosenConsumables.Count ? chosenConsumables[index] : null;

    public bool UseConsumable(int slotIndex, PlayerBaseEntity user)
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
                chosenConsumables[slotIndex] = null;
                runtimeData[slotIndex] = null;
            }

            NotifyChanged();
            return true;
        }

        return false;
    }
    public bool EquipConsumables(int slotIndex, ConsumableDataSO dataSO)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return false;

        EnsureSlotCount();

        int existing = chosenConsumables.IndexOf(dataSO);
        if (existing == slotIndex)
            return true;

        ConsumableDataSO displaced = chosenConsumables[slotIndex];
        chosenConsumables[slotIndex] = dataSO;

        if (existing >= 0)
            chosenConsumables[existing] = displaced;

        InitializeRuntime();
        NotifyChanged();
        return true;
    }

    public bool RemoveConsumablesAt(int slotindex)
    {
        if (slotindex < 0 || slotindex >= chosenConsumables.Count) return false;
        if (chosenConsumables[slotindex] == null) return false;

        chosenConsumables[slotindex] = null;
        NotifyChanged();
        return true;
    }

    public void ClearAll()
    {
        bool isChanged = false;
        for (int i = 0; i < chosenConsumables.Count; i++)
        {
            if (chosenConsumables[i] == null) return;
            chosenConsumables[i] = null;
            isChanged = true;
        }

        if (isChanged) NotifyChanged();
    }

    public void NotifyChanged() => OnChosenConsumablesChanged?.Invoke();
    public void EnsureSlotCount()
    {
        while (chosenConsumables.Count < MaxSlots)
            chosenConsumables.Add(null);

        if (chosenConsumables.Count > MaxSlots)
            chosenConsumables.RemoveRange(MaxSlots, chosenConsumables.Count - MaxSlots);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < MaxSlots; i++)
            if (chosenConsumables[i] == null) return i;

        return -1;
    }
}