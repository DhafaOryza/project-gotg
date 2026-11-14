using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [Header("References")]
    [SerializeField] private HUD _playerHUD;
    [SerializeField] private PreparationUI preparationUI;
    [SerializeField] private ResultUI resultUI;
    [SerializeField] private DayTransitionUI dayTransition;

    [Header("Input System UI")]
    [SerializeField] private InputActionAsset inputAction;
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string controllerBindingGroup = "Controller";
    [SerializeField] private string keyboardBindingGroup = "Keyboard";
    [SerializeField] private InputSystemUIInputModule uIInputModule;

    private InputActionAsset previousActionsAsset;
    private InputActionReference previousNavigateAction;
    private InputActionReference previousSubmitAction;
    private InputActionReference previousCancelAction;
    private InputActionReference previousPointAction;
    private InputActionReference previousClickAction;
    private InputActionReference previousRightClickAction;
    private InputActionReference previousMiddleClickAction;
    private InputActionReference previousScrollWheelAction;
    private InputActionMap uiActionMap;
    private InputAction uiSubmitAction;
    private InputAction uiCancelAction;
    private readonly System.Collections.Generic.List<InputActionReference> createdActionReferences =
        new System.Collections.Generic.List<InputActionReference>();
    private bool isNavigationInitialized;
    private bool isUIInputActive;

    public HUD HUD => _playerHUD;
    public PreparationUI Preparation => preparationUI;
    public ResultUI ResultUI => resultUI;
    public DayTransitionUI DayTransition => dayTransition;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        SetInputMode(false);
        RestoreUINavigation();
        if (Instance == this)
            Instance = null;
    }

    public void Initialize()
    {
        if (_playerHUD == null) _playerHUD = FindFirstObjectByType<HUD>();
        if (preparationUI == null) preparationUI = FindFirstObjectByType<PreparationUI>();
        if (resultUI == null) resultUI = FindFirstObjectByType<ResultUI>();
        if (dayTransition == null) dayTransition = FindFirstObjectByType<DayTransitionUI>();

        if (_playerHUD != null) _playerHUD.Initialize();

        InitializeUINavigation();

        DeactivateAllPanel();
    }

    public void ActivatePreparationUI()
    {
        if (preparationUI == null) return;

        preparationUI.ActivateSelf();
        SetInputMode(true);
        SetSelectedObject(preparationUI.FirstSelectedButton);
    }

    public void DeactivatePreparationUI()
    {
        if (preparationUI != null)
        {
            preparationUI.DeactivatePreparePanel();
            preparationUI.DeactivateSelf();
        }

        SetInputMode(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void ActivateResultUI(NightResult result)
    {
        if (resultUI == null)
        {
            Debug.LogError("[PlayerUI] resultUI belum di-assign, ResultUI tidak bisa ditampilkan.", this);
            return;
        }

        resultUI.gameObject.SetActive(true);
        resultUI.Show(result);

        // Hasil night berakhir saat Combat: UI input mode masih mati, jadi klik/Submit tidak sampai ke tombol.
        SetInputMode(true);
        SetSelectedObject(resultUI.ContinueButton);
    }
    public void DeactivateResultUI() => resultUI.gameObject.SetActive(false);

    public void SelectPreparationFirstSelected()
    {
        if (preparationUI != null)
            SetSelectedObject(preparationUI.FirstSelectedButton);
    }

    public void SetSelectedObject(Selectable selectedObject)
    {
        if (selectedObject == null || !selectedObject.isActiveAndEnabled || !selectedObject.IsInteractable())
            return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning("PlayerUI: EventSystem tidak ditemukan, object tidak dapat dipilih.", this);
            return;
        }

        EventSystem.current.SetSelectedGameObject(selectedObject.gameObject);
    }

    public void BindGranary(IInteractable interactable)
    {
        if (interactable is GranaryBase granary)
        {
            granary.OnInteract -= ActivatePreparationUI;
            granary.OnInteract += ActivatePreparationUI;
        }
    }

    #region Helper

    public void InitializeUINavigation()
    {
        if (isNavigationInitialized) return;

        if (inputAction == null)
        {
            Debug.LogError("[PlayerUI] Input Action Asset belum di-assign.", this);
            return;
        }

        var actionMap = inputAction.FindActionMap(uiActionMapName, false);
        if (actionMap == null)
        {
            Debug.LogError($"[PlayerUI] Action Map '{uiActionMapName}' tidak ditemukan.", this);
            return;
        }

        WarnIfBindingGroupMissing(actionMap, controllerBindingGroup);
        WarnIfBindingGroupMissing(actionMap, keyboardBindingGroup);
        uiActionMap = actionMap;

        uiSubmitAction = actionMap.FindAction("Submit", false);
        uiCancelAction = actionMap.FindAction("Cancel", false);
        if (uiSubmitAction == null || uiCancelAction == null)
        {
            Debug.LogError($"[PlayerUI] Action Submit dan/atau Cancel tidak ditemukan pada map '{uiActionMapName}'.", this);
            uiActionMap = null;
            return;
        }

        if (uIInputModule == null && EventSystem.current != null)
            uIInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

        if (uIInputModule == null)
        {
            Debug.LogError("[PlayerUI] InputSystemUIInputModule tidak ditemukan.", this);
            return;
        }

        previousActionsAsset = uIInputModule.actionsAsset;
        previousNavigateAction = uIInputModule.move;
        previousSubmitAction = uIInputModule.submit;
        previousCancelAction = uIInputModule.cancel;
        previousPointAction = uIInputModule.point;
        previousClickAction = uIInputModule.leftClick;
        previousRightClickAction = uIInputModule.rightClick;
        previousMiddleClickAction = uIInputModule.middleClick;
        previousScrollWheelAction = uIInputModule.scrollWheel;

        uIInputModule.actionsAsset = inputAction;
        AssignModuleAction(actionMap, "Navigate", action => uIInputModule.move = action);
        // Submit dan Cancel ditangani oleh PlayerUI agar tidak dieksekusi dua kali oleh Input Module.
        uIInputModule.submit = null;
        uIInputModule.cancel = null;
        AssignModuleAction(actionMap, "Point", action => uIInputModule.point = action);
        AssignModuleAction(actionMap, "Click", action => uIInputModule.leftClick = action);
        AssignModuleAction(actionMap, "RightClick", action => uIInputModule.rightClick = action);
        AssignModuleAction(actionMap, "MiddleClick", action => uIInputModule.middleClick = action);
        AssignModuleAction(actionMap, "ScrollWheel", action => uIInputModule.scrollWheel = action);

        uiSubmitAction.performed += HandleUISubmit;
        uiCancelAction.performed += HandleUICancel;

        // Semua binding pada asset tetap aktif, termasuk kelompok Controller dan Keyboard.
        isNavigationInitialized = true;
    }

    private void AssignModuleAction(
        InputActionMap actionMap,
        string actionName,
        System.Action<InputActionReference> assignAction)
    {
        var action = actionMap.FindAction(actionName, false);
        if (action == null)
        {
            Debug.LogWarning(
                $"[PlayerUI] Action '{actionName}' tidak ditemukan pada map '{uiActionMapName}'.",
                this);
            return;
        }

        var actionReference = InputActionReference.Create(action);
        if (actionReference == null)
        {
            Debug.LogWarning($"[PlayerUI] Gagal membuat reference untuk action '{actionName}'.", this);
            return;
        }

        createdActionReferences.Add(actionReference);
        assignAction(actionReference);
    }

    private void WarnIfBindingGroupMissing(InputActionMap actionMap, string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName)) return;

        foreach (var binding in actionMap.bindings)
        {
            if (string.IsNullOrEmpty(binding.groups)) continue;

            foreach (string group in binding.groups.Split(';'))
            {
                if (string.Equals(group.Trim(), groupName, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }
        }

        Debug.LogWarning(
            $"[PlayerUI] Tidak ada binding dari group '{groupName}' pada action map '{uiActionMapName}'.",
            this);
    }

    private void RestoreUINavigation()
    {
        if (!isNavigationInitialized) return;

        if (uiSubmitAction != null) uiSubmitAction.performed -= HandleUISubmit;
        if (uiCancelAction != null) uiCancelAction.performed -= HandleUICancel;

        if (uIInputModule != null)
        {
            uIInputModule.actionsAsset = previousActionsAsset;
            uIInputModule.move = previousNavigateAction;
            uIInputModule.submit = previousSubmitAction;
            uIInputModule.cancel = previousCancelAction;
            uIInputModule.point = previousPointAction;
            uIInputModule.leftClick = previousClickAction;
            uIInputModule.rightClick = previousRightClickAction;
            uIInputModule.middleClick = previousMiddleClickAction;
            uIInputModule.scrollWheel = previousScrollWheelAction;
        }

        foreach (var actionReference in createdActionReferences)
        {
            if (actionReference != null)
                Destroy(actionReference);
        }

        createdActionReferences.Clear();
        uiActionMap = null;
        uiSubmitAction = null;
        uiCancelAction = null;
        isNavigationInitialized = false;
    }

    private void HandleUISubmit(InputAction.CallbackContext context)
    {
        if (!isUIInputActive || EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || !selected.activeInHierarchy) return;

        var eventData = new BaseEventData(EventSystem.current);
        ExecuteEvents.Execute(selected, eventData, ExecuteEvents.submitHandler);
    }

    private void HandleUICancel(InputAction.CallbackContext context)
    {
        if (!isUIInputActive || preparationUI == null) return;

        if (preparationUI.IsPreparePanelOpen)
        {
            preparationUI.DeactivatePreparePanel();
            SelectPreparationFirstSelected();
            return;
        }

        DeactivatePreparationUI();
    }

    private void SetInputMode(bool useUIInput)
    {
        isUIInputActive = useUIInput;

        if (uiActionMap != null)
        {
            if (useUIInput)
                uiActionMap.Enable();
            else
                uiActionMap.Disable();
        }

        var playerController = GameManager.Instance?.playerController;
        if (playerController != null)
            playerController.SetGameplayInputEnabled(!useUIInput);
    }

    private void DeactivateAllPanel()
    {
        DeactivatePreparationUI();
    }

    #endregion
}
