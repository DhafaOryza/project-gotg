using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ConsumableRadialUI : MonoBehaviour
{
    [Header("Radial Slot Bindings")]
    [SerializeField] private List<RadialSlotBinding> slots = new List<RadialSlotBinding>
    {
        new RadialSlotBinding { slotName = "Top", triggerKey = KeyCode.Alpha1 },
        new RadialSlotBinding { slotName = "Right", triggerKey = KeyCode.Alpha2 },
        new RadialSlotBinding { slotName = "Left", triggerKey = KeyCode.Alpha3 },
        new RadialSlotBinding { slotName = "Bottom", triggerKey = KeyCode.Alpha4 }
    };

    private PlayerBaseEntity player;

    private void Start()
    {
        player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
        RefreshAllSlots();
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
            if (player == null || !player.CanAct) return;
        }

        if (!player.CanAct) return;

        HandleInput();
    }

    private void HandleInput()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (ReadDigitDown(slots[i].triggerKey))
            {
                UseSlot(i);
                break;
            }
        }
    }

    /// <summary>
    /// Eksekusi consumable pada slot tertentu
    /// </summary>
    public void UseSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        var slot = slots[index];

        if (slot.boundData == null)
        {
            Debug.Log($"[RadialUI] Slot {slot.slotName} kosong.");
            return;
        }

        bool success = slot.boundData.Use(player);
        if (success)
        {
            slot.currentQuantity--;
            if (slot.currentQuantity <= 0)
                SetSlotData(index, null, 0);
        }
    }

    /// <summary>
    /// Update/Pasang data consumable ke slot tertentu secara dinamis
    /// </summary>
    public void SetSlotData(int slotIndex, ConsumableDataSO data, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        slots[slotIndex].boundData = data;
        slots[slotIndex].currentQuantity = quantity;
        UpdateSlotVisual(slots[slotIndex]);
    }

    /// <summary>
    /// Refresh seluruh tampilan ikon di UI
    /// </summary>
    public void RefreshAllSlots()
    {
        foreach (var slot in slots)
        {
            UpdateSlotVisual(slot);
        }
    }

    private void UpdateSlotVisual(RadialSlotBinding slot)
    {
        if (slot.slotIconImage == null) return;
        if (slot.boundData == null || slot.boundData.icon == null)
        {
            slot.slotIconImage.gameObject.SetActive(false);
            return;
        }

        slot.slotIconImage.sprite = slot.boundData.icon;
        slot.slotIconImage.gameObject.SetActive(true);
    }

    #region Input Helper (Support Old & New Input System)

    private bool ReadDigitDown(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return false;
        string keyName = key.ToString().Replace("Alpha", "Digit");
        if (Enum.TryParse(keyName, out Key parsedKey) && parsedKey != Key.None)
            return Keyboard.current[parsedKey].wasPressedThisFrame;
        return false;
#else
        return Input.GetKeyDown(key);
#endif
    }

    #endregion
}