using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ItemRadialUI : MonoBehaviour
{
    [Header("Radial Slot Bindings")]
    [SerializeField] private List<RadialSlotBinding> slots = new List<RadialSlotBinding>
    {
        new RadialSlotBinding { slotName = "Top", triggerKey = KeyCode.Alpha1 },
        new RadialSlotBinding { slotName = "Right", triggerKey = KeyCode.Alpha2 },
        new RadialSlotBinding { slotName = "Left", triggerKey = KeyCode.Alpha3 },
        new RadialSlotBinding { slotName = "Bottom", triggerKey = KeyCode.Alpha4 }
    };
    
    // Diubah menggunakan ItemRadialSlotUI khusus radial
    [SerializeField] private ItemRadialSlotUI slotPrefabsUI;

    private PlayerBaseEntity player;
    private ItemManager itemManager;
    private bool isSubscribedToManager;

    private void Awake()
    {
        InitializeSocket();
    }

    private void Start()
    {
        player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
        EnsureItemManager();
        SyncWithManager();  
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

    private void OnEnable()
    {
        EnsureItemManager();
        SubscribeToManager();
        SyncWithManager();
    }

    private void OnDisable()
    {
        UnsubscribeFromManager();
    }

    private void EnsureItemManager()
    {
        ItemManager manager = GameManager.Instance?.itemManager;
        if (manager == null)
            manager = FindAnyObjectByType<ItemManager>();

        if (itemManager == manager) return;

        UnsubscribeFromManager();

        itemManager = manager;

        if (isActiveAndEnabled)
            SubscribeToManager();
    }

    private void SubscribeToManager()
    {
        if (itemManager == null || isSubscribedToManager) return;

        itemManager.OnChosenItemsChanged += SyncWithManager;
        isSubscribedToManager = true;
    }

    private void UnsubscribeFromManager()
    {
        if (itemManager == null || !isSubscribedToManager) return;

        itemManager.OnChosenItemsChanged -= SyncWithManager;
        isSubscribedToManager = false;
    }

    private void InitializeSocket()
    {
        if (slotPrefabsUI == null) return;

        foreach (var slot in slots)
        {
            if (slot.socketParent == null) continue;

            foreach (Transform child in slot.socketParent)
                Destroy(child.gameObject);

            var instance = Instantiate(slotPrefabsUI, slot.socketParent);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Store the reference so SetSlotData can call Setup() on it.
            slot.spawnedSlotUI = instance;

            // Hide immediately so the slot does not show placeholder content before SyncWithManager runs.
            instance.gameObject.SetActive(false);
        }
    }

    public void SyncWithManager()
    {
        EnsureItemManager();
        if (itemManager == null) return;

        for (int i = 0; i < slots.Count && i < ItemManager.MaxSlots ; i++)
        {
            var runtime = itemManager.GetRuntimeSlot(i);
            if (runtime != null && runtime.HasStock)
            {
                SetSlotData(i, runtime.data, runtime.currentStack);
            }
            else
            {
                SetSlotData(i, null, 0);
            }
        }
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

    public void UseSlot(int index)
    {
        if (itemManager == null || player == null) return;
        itemManager.UseItem(index, player);
    }

    public void SetSlotData(int slotIndex, ItemDataSO data, int currentStack = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        slot.boundData = data;

        if (slot.spawnedSlotUI != null)
        {
            slot.spawnedSlotUI.Setup(data, currentStack);
        }
    }

    #region Input Helper

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
