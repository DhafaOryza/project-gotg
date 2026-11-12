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

    [Header("Input System UI")]
    [SerializeField] private InputActionAsset inputAction;
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string controllerBindingGroup = "Controller";
    [SerializeField] private string keyboardBindingGroup = "Keyboard";
    [SerializeField] private InputSystemUIInputModule uIInputModule;

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

    public void Initialize()
    {
        if (_playerHUD == null) _playerHUD = FindFirstObjectByType<HUD>();
        if (preparationUI == null) preparationUI = FindFirstObjectByType<PreparationUI>();

        if (_playerHUD != null) _playerHUD.Initialize();

        DeactivateAllPanel();
    }

    public void ActivatePreparationUI()
    {
        if (preparationUI == null) return;

        preparationUI.ActivateSelf();
        SetSelectedObject(preparationUI.FirstSelectedButton);
    }

    public void DeactivatePreparationUI()
    {
        if (preparationUI != null)
            preparationUI.DeactivateSelf();
    }

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

    private void DeactivateAllPanel()
    {
        DeactivatePreparationUI();
    }

    #endregion
}
