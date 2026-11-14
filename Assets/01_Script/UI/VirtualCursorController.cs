using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class VirtualCursorController : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float cursorSpeed = 1200f;

    [Header("Input Action")]
    [Tooltip("Action 'Navigate - Virtual Cursor' (Vector2 Pass Through)")]
    [SerializeField] private InputActionReference cursorMoveAction;

    [Header("Modular Toggle Settings")]
    [Tooltip("Pilih tombol gamepad dari dropdown (default: LeftTrigger / L2)")]
    [SerializeField] private GamepadButton gamepadToggleButton = GamepadButton.Select;
    
    [Tooltip("Tombol keyboard alternatif untuk testing (default: Z)")]
    [SerializeField] private Key keyboardToggleKey = Key.Z;

    private bool isActive = false;
    private Vector2 currentScreenPosition;
    private InputAction activeCursorAction;

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        currentScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        SetCursorActive(false);
    }

    private void Start()
    {
        BindActiveAction();
    }

    private void OnEnable()
    {
        if (cursorMoveAction != null)
            cursorMoveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (cursorMoveAction != null)
            cursorMoveAction.action.Disable();
    }

    private void Update()
    {
        if (activeCursorAction == null)
            BindActiveAction();

        if (Gamepad.current != null && CheckGamepadButtonPressed(gamepadToggleButton))
            SetCursorActive(!isActive);
        
        if (Keyboard.current != null && Keyboard.current[keyboardToggleKey].wasPressedThisFrame)
            SetCursorActive(!isActive);

        if (!isActive) return;

        Vector2 stickInput = Vector2.zero;
        if (activeCursorAction  != null)
            stickInput = activeCursorAction.ReadValue<Vector2>();

        if (stickInput.sqrMagnitude > 0.01f)
        {
            currentScreenPosition += stickInput * (cursorSpeed * Time.unscaledDeltaTime);

            currentScreenPosition.x = Mathf.Clamp(currentScreenPosition.x, 0f, Screen.width);
            currentScreenPosition.y = Mathf.Clamp(currentScreenPosition.y, 0f, Screen.height);

            UpdateCursorVisualPosition();

            if (Mouse.current != null)
                InputState.Change(Mouse.current.position, currentScreenPosition);
        }
    }

    private void UpdateCursorVisualPosition()
    {
        if (cursorTransform == null || rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            currentScreenPosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out Vector2 localPoint
        );

        cursorTransform.localPosition = localPoint;
    }

    private void BindActiveAction()
    {
        var PlayerController = GameManager.Instance?.playerController;
        if (PlayerController != null && PlayerController.VirtualCursorAction != null)
        {
            activeCursorAction = PlayerController.VirtualCursorAction;
            activeCursorAction.Enable();
            Debug.Log("<color=green>[VirtualCursor] Berhasil terhubung ke Action 'Virtual Cursor' PlayerController!</color>");
        }
        else if (cursorMoveAction != null)
        {
            activeCursorAction = cursorMoveAction.action;
            activeCursorAction.Enable();
        }
    }

    public void SetCursorActive(bool active)
    {
        isActive = active;

        if (cursorTransform != null)
            cursorTransform.gameObject.SetActive(active);

        if (active)
        {
            if (Mouse.current != null)
                currentScreenPosition = Mouse.current.position.ReadValue();
            else
                currentScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            UpdateCursorVisualPosition();
        }
    }

    private bool CheckGamepadButtonPressed(GamepadButton button)
    {
        if (Gamepad.current == null) return false;
        var btn = Gamepad.current[button];
        return btn != null && btn.wasPressedThisFrame;
    }
}