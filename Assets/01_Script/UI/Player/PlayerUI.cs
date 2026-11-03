using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [Header("Global Volume")]
    [SerializeField] private Volume _globalVolumeUI;

    [Header("Player Profile")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar granaryHealthBar;

    [Header("Preparation")]
    [SerializeField] private PreparationUI _preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform _skillContainer;
    [SerializeField] private SkillUI _skillUIPrefab;

    [Header("Consumable & Item Radial Menu")]
    [SerializeField] private GameObject _consumableRadialContainer;
    [SerializeField] private ConsumableRadialUI _consumableRadialUI;

    [Header("Dialog System")]
    [SerializeField] private ConversationUI _conversationUI;

    [Header("Role NPC Panels")]
    [SerializeField] private BuilderPanelUI _builderPanelUI;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private LevelAndStatsUI _statsStore;
    [SerializeField] private SkillTreeUI _skillTreeUI;

    [Header("Result")]
    [SerializeField] private ResultUI _resultUI;

    [Header("Transition")]
    [SerializeField] private DayTransitionUI dayTransition;

    [Header("UI Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string controllerBindingGroup = "Controller";
    [SerializeField] private string keyboardBindingGroup = "Keyboard";
    [SerializeField] private InputSystemUIInputModule uiInputModule;


    private SkillsManager _skillsManager;
    private PlayerController _playerController;
    private PlayerBaseEntity player;
    private GranaryBase _granary;
    private NPCDataSO _npcData;
    private NPCMovement _npcMovement;
    private NPCBaseEntity _activeNpc;
    private InputActionAsset runtimeUIInputActions;
    private InputAction uiSubmitAction;
    private InputAction uiCancelAction;
    private string activeUIBindingGroup;
    private Coroutine panelFocusRoutine;

    public ConversationUI ConversationUI => _conversationUI;
    public BuilderPanelUI BuilderPanel => _builderPanelUI;
    public ShopUI ShopUI => _shopUI;
    public LevelAndStatsUI StatsStore => _statsStore;
    public SkillTreeUI SkillTreeUI => _skillTreeUI;
    public ResultUI ResultUI => _resultUI;
    public DayTransitionUI DayTransition => dayTransition;
    public NPCDataSO NpcData => _npcData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeUI();
    }

    void OnEnable()
    {
        InputSystem.onDeviceChange += HandleUIDeviceChange;

        if (_granary != null)
            _granary.OnInteract += OpenPreparationUI;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleUIDeviceChange;

        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;
    }

    void OnDestroy()
    {
        if (uiSubmitAction != null)
            uiSubmitAction.performed -= HandleUISubmit;

        if (uiCancelAction != null)
            uiCancelAction.performed -= HandleUICancel;

        if (uiInputModule != null && uiInputModule.actionsAsset == runtimeUIInputActions)
            uiInputModule.UnassignActions();

        if (runtimeUIInputActions != null)
            Destroy(runtimeUIInputActions);
    }

    /// <summary>
    /// Sambungkan event OnInteract milik granary ke PreparationUI.
    /// Dipanggil oleh GranaryBase saat Start (atau oleh PlayerUI.Start untuk granary yang sudah ada).
    /// </summary>
    public void BindGranary(GranaryBase granary)
    {
        if (_granary == granary) return;

        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;

        _granary = granary;

        if (_granary != null && isActiveAndEnabled)
            _granary.OnInteract += OpenPreparationUI;
    }

    /// <summary>
    /// Initializes all core user interface components at game start.
    /// </summary>
    public void InitializeUI()
    {
        InitializeUIInput();

        if (_globalVolumeUI == null) _globalVolumeUI = GetComponent<Volume>();

        if (_preparationUI == null) _preparationUI = Object.FindFirstObjectByType<PreparationUI>();
        if (_conversationUI == null) _conversationUI = Object.FindFirstObjectByType<ConversationUI>();
        if (_builderPanelUI == null) _builderPanelUI = Object.FindFirstObjectByType<BuilderPanelUI>();
        if (_statsStore == null) _statsStore = Object.FindFirstObjectByType<LevelAndStatsUI>();
        if (_shopUI == null) _shopUI = Object.FindFirstObjectByType<ShopUI>();
        if (_skillTreeUI == null) _skillTreeUI = Object.FindFirstObjectByType<SkillTreeUI>();
        if (_resultUI == null) _resultUI = Object.FindFirstObjectByType<ResultUI>();
        if (_resultUI == null) _resultUI = Object.FindFirstObjectByType<ResultUI>(FindObjectsInactive.Include);
        if (dayTransition == null) dayTransition = Object.FindFirstObjectByType<DayTransitionUI>();

        if (_preparationUI != null) _preparationUI.gameObject.SetActive(false);
        if (_skillContainer != null) _skillContainer.gameObject.SetActive(false);
        if (_consumableRadialContainer != null) _consumableRadialContainer.gameObject.SetActive(false);

        CloseAllRolePanels();

        if (_granary == null)
        {
            var granary = GameManager.Instance?.levelSpawnerManager?.GetCurrentGranary();
            if (granary != null) BindGranary(granary);
        }

        if (player == null)
        {
            var currentPlayer = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
            player = currentPlayer;
        }

        InitializeHealthBar();
        InitializeVolume();
    }

    private void InitializeUIInput()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("[PlayerUI] Input Actions asset belum di-assign; navigasi gamepad UI tidak diaktifkan.", this);
            return;
        }

        if (uiInputModule == null)
        {
            var eventSystem = EventSystem.current != null
                ? EventSystem.current
                : Object.FindFirstObjectByType<EventSystem>();

            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            uiInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (uiInputModule == null)
            {
                foreach (var module in eventSystem.GetComponents<BaseInputModule>())
                    module.enabled = false;

                uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        if (runtimeUIInputActions == null)
            runtimeUIInputActions = Instantiate(inputActions);

        if (uiSubmitAction != null)
            uiSubmitAction.performed -= HandleUISubmit;

        if (uiCancelAction != null)
            uiCancelAction.performed -= HandleUICancel;

        uiInputModule.AssignDefaultActions();
        uiInputModule.actionsAsset = runtimeUIInputActions;

        var uiActionMap = runtimeUIInputActions.FindActionMap(uiActionMapName, false);
        if (uiActionMap == null)
        {
            Debug.LogError($"[PlayerUI] Action Map '{uiActionMapName}' tidak ditemukan.", this);
            return;
        }

        uiSubmitAction = uiActionMap.FindAction("Submit", false);
        if (uiSubmitAction != null)
            uiSubmitAction.performed += HandleUISubmit;

        uiCancelAction = uiActionMap.FindAction("Cancel", false);
        if (uiCancelAction != null)
            uiCancelAction.performed += HandleUICancel;

        UpdateUIBindingGroupForConnectedDevices();
    }

    private void HandleUICancel(InputAction.CallbackContext context)
    {
        if (_conversationUI != null && _conversationUI.IsDialogOpen)
        {
            _conversationUI.OnContinueClicked();
            return;
        }

        if (_preparationUI != null && _preparationUI.gameObject.activeInHierarchy)
        {
            // Tutup subpanel terlebih dahulu agar Cancel berperilaku seperti tombol Back.
            if (_preparationUI.TryClosePrepareSkillPanel())
            {
                return;
            }

            ClosePreparationUI();
            return;
        }

        if (IsPanelOpen(_statsStore) || IsPanelOpen(_builderPanelUI) ||
            IsPanelOpen(_shopUI) || IsPanelOpen(_skillTreeUI))
        {
            CloseAllRolePanels();
            return;
        }

        if (_consumableRadialContainer != null && _consumableRadialContainer.activeInHierarchy)
        {
            CloseConsumableUI();
            return;
        }

        if (_skillContainer != null && _skillContainer.gameObject.activeInHierarchy)
            CloseSkillUI();
    }

    private void HandleUISubmit(InputAction.CallbackContext context)
    {
        if (_conversationUI != null && _conversationUI.IsDialogOpen)
            _conversationUI.OnContinueClicked();
    }

    private static bool IsPanelOpen(Component panel)
    {
        return panel != null && panel.gameObject.activeInHierarchy;
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        var eventSystem = uiInputModule != null
            ? uiInputModule.GetComponent<EventSystem>()
            : EventSystem.current;
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);

        panel.SetActive(true);

        if (panelFocusRoutine != null)
            StopCoroutine(panelFocusRoutine);

        panelFocusRoutine = StartCoroutine(FocusPanelNextFrame(panel));
    }

    private IEnumerator FocusPanelNextFrame(GameObject panel)
    {
        yield return null;

        panelFocusRoutine = null;
        if (panel != null && panel.activeInHierarchy)
            FocusFirstSelectable(panel);
    }

    private void FocusFirstSelectable(GameObject panel)
    {
        if (panel == null) return;

        var eventSystem = uiInputModule != null
            ? uiInputModule.GetComponent<EventSystem>()
            : EventSystem.current;
        if (eventSystem == null) return;

        foreach (var selectable in panel.GetComponentsInChildren<Selectable>(false))
        {
            if (selectable == null || !selectable.IsInteractable() || !selectable.gameObject.activeInHierarchy)
                continue;

            eventSystem.SetSelectedGameObject(selectable.gameObject);
            return;
        }
    }

    private void HandleUIDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            SetUIBindingGroup(controllerBindingGroup);
            return;
        }

        if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected ||
            change == InputDeviceChange.Disabled)
        {
            UpdateUIBindingGroupForConnectedDevices(device);
            return;
        }

        UpdateUIBindingGroupForConnectedDevices();
    }

    private void UpdateUIBindingGroupForConnectedDevices(InputDevice unavailableDevice = null)
    {
        bool hasGamepad = false;
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad != unavailableDevice && gamepad.added && gamepad.enabled)
            {
                hasGamepad = true;
                break;
            }
        }

        SetUIBindingGroup(hasGamepad ? controllerBindingGroup : keyboardBindingGroup);
    }

    private void SetUIBindingGroup(string bindingGroup)
    {
        if (runtimeUIInputActions == null || activeUIBindingGroup == bindingGroup)
            return;

        runtimeUIInputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);
        activeUIBindingGroup = bindingGroup;
    }

    private void InitializeVolume()
    {
        DeactivateVolume();
    }

    /// <summary>
    /// Opens the corresponding panel automatically based on the NPC data profile and tracks active movement.
    /// </summary>
    public void HandleNpcRolePanel(NPCDataSO npcData, NPCMovement npcMovement, NPCBaseEntity npc = null)
    {
        CloseAllRolePanels();

        if (npcData == null) return;

        _activeNpc = npc;
        _npcMovement = npcMovement;

        switch (npcData.NpcRole)
        {
            case NpcRole.Priest:
                if (_statsStore != null)
                {
                    _statsStore.Initialize();
                    OpenPanel(_statsStore.gameObject);
                }
                break;
            case NpcRole.GranaryKeeper:
                if (_builderPanelUI != null)
                {
                    _builderPanelUI.Initialize();
                    OpenPanel(_builderPanelUI.gameObject);
                }
                break;
            case NpcRole.Merchant:
                if (_shopUI != null)
                {
                    _shopUI.Initialize();
                    OpenPanel(_shopUI.gameObject);
                }
                break;
            case NpcRole.Guard:
            case NpcRole.GateGuard:
                if (_skillTreeUI != null)
                {
                    _skillTreeUI.Initialize(npcData);
                    OpenPanel(_skillTreeUI.gameObject);
                }
                break;
            default:
                if (_npcMovement != null)
                {
                    _npcMovement.ResumeMovement();
                }
                _activeNpc = null;
                _npcMovement = null;
                break;
        }
    }

    /// <summary>
    /// Closes all role-specific UI panels and resumes active NPC movement.
    /// </summary>
    public void CloseAllRolePanels()
    {
        CloseStatsStore();
        CloseGranaryUI();
        CloseShopUI();
        CloseSkillTreeUI();

        if (_npcMovement != null)
        {
            _npcMovement.ResumeMovement();
            _npcMovement = null;
            _activeNpc = null;
        }
    }

    public void InitializeHealthBar()
    {
        playerHealthBar.Initialize(player);
        granaryHealthBar.Initialize(_granary);
    }

    public void InitializeSkillUI()
    {
        _skillsManager = GameManager.Instance?.skillsManager;
        _playerController = GameManager.Instance?.playerController;

        if (_skillsManager == null || _playerController == null || _skillUIPrefab == null || _skillContainer == null)
            return;

        foreach (Transform child in _skillContainer)
        {
            Destroy(child.gameObject);
        }

        var skills = _skillsManager.choosenSkills;
        var keys = new List<string>
        {
            _playerController.GetSkillDisplayName(0),
            _playerController.GetSkillDisplayName(1),
            _playerController.GetSkillDisplayName(2),
            _playerController.GetSkillDisplayName(3),
        };

        for (int i = 0; i < skills.Count && i < keys.Count; i++)
        {
            SkillUI currentSkill = Instantiate(_skillUIPrefab, _skillContainer);
            currentSkill.Setup(i, keys[i], skills[i]);
        }
    }

    public void ActivateVolume()
    {
        if (_globalVolumeUI == null) return;

        _globalVolumeUI.weight = 1f;
    }

    public void DeactivateVolume()
    {
        if (_globalVolumeUI == null) return;

        _globalVolumeUI.weight = 0f;
    }

    public void OpenPreparationUI()
    {
        if (_preparationUI != null)
            OpenPanel(_preparationUI.gameObject);
    }

    public void ClosePreparationUI()
    {
        _preparationUI?.gameObject.SetActive(false);

        if (_granary != null)
            _granary.ResumeSign();
    }

    public void OpenStatsStore() => OpenPanel(_statsStore != null ? _statsStore.gameObject : null);
    public void CloseStatsStore() => _statsStore?.gameObject.SetActive(false);

    public void OpenGranaryUI() => OpenPanel(_builderPanelUI != null ? _builderPanelUI.gameObject : null);
    public void CloseGranaryUI() => _builderPanelUI?.gameObject.SetActive(false);

    public void OpenShopUI() => OpenPanel(_shopUI != null ? _shopUI.gameObject : null);
    public void CloseShopUI() => _shopUI?.gameObject.SetActive(false);

    public void OpenSkillTreeUI() => OpenPanel(_skillTreeUI != null ? _skillTreeUI.gameObject : null);
    public void CloseSkillTreeUI() => _skillTreeUI?.gameObject.SetActive(false);

    public void OpenSkillUI() => OpenPanel(_skillContainer != null ? _skillContainer.gameObject : null);
    public void CloseSkillUI() => _skillContainer?.gameObject.SetActive(false);

    public void OpenConsumableUI()
    {
        if (_consumableRadialContainer != null)
            OpenPanel(_consumableRadialContainer);
        if (_consumableRadialUI != null)
            _consumableRadialUI.SyncWithManager();
    }
    public void CloseConsumableUI() => _consumableRadialContainer?.gameObject.SetActive(false);
    public void OpenResultUI(NightResult result)
    {
        if (_resultUI == null)
        {
            Debug.LogWarning("[PlayerUI] ResultUI tidak ditemukan di scene.");
            return;
        }

        _resultUI.gameObject.SetActive(true);
        _resultUI.Show(result);
        FocusFirstSelectable(_resultUI.gameObject);
    }
    public void CloseResultUI() => _resultUI?.gameObject.SetActive(false);

    internal void HandleNpcRolePanel(string npcId)
    {
        throw new System.NotImplementedException();
    }
}
