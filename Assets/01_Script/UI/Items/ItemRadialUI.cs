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
    [SerializeField] private ItemSlotUI slotPrefabsUI;

    private PlayerBaseEntity player;
    private ItemManager itemManager;

    private void Awake()
    {
        InitializeSocket();
    }
    private void Start()
    {
        player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
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
        itemManager = GameManager.Instance?.itemManager;
        if (itemManager == null)
            itemManager = FindAnyObjectByType<ItemManager>();

        if (itemManager != null)
        {
            itemManager.OnChosenItemsChanged += SyncWithManager;
            SyncWithManager();
        }

    }
    private void OnDisable()
    {
        if (itemManager != null)
            itemManager.OnChosenItemsChanged -= SyncWithManager;
    }

    private void InitializeSocket()
    {
        if (slotPrefabsUI == null) return;

        foreach (var slot in slots)
        {
            if (slot.socketParent != null)
            {
                foreach (Transform child in slot.socketParent)
                    Destroy(child.gameObject);

                var Instance = Instantiate(slotPrefabsUI, slot.socketParent);

                Instance.transform.localPosition = Vector3.zero;
                Instance.transform.localRotation = Quaternion.identity;
                Instance.transform.localScale = Vector3.one;

                slot.spawnedSlotUI = Instance;
                slot.spawnedSlotUI.gameObject.SetActive(false);
            }
        }
    }

    public void SyncWithManager()
    {
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

    /// <summary>
    /// Eksekusi item pada slot tertentu
    /// </summary>
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
            slot.spawnedSlotUI.Setup(data, currentStack);
    }

    public void LoadFromGameSession()
    {
        var sessionData = GameSessionData.Instance;
        if (sessionData == null || sessionData.items == null) return;

        for (int i = 0; i < slots.Count ; i++)
        {
            if (i < sessionData.items.Count)
                SetSlotData(i, sessionData.items[i]);
            else
                SetSlotData(i, null);
        }

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