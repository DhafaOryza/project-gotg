using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class VirtualCursorController : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float cursorSpeed = 1000f;

    [Header("Input Action")]
    [Tooltip("Action 'Navigate - Virtual Cursor' (Vector2 Pass Through)")]
    [SerializeField] private InputActionReference cursorMoveAction;

    [Header("Modular Toggle Settings")]
    [Tooltip("Pilih tombol gamepad dari dropdown (default: LeftTrigger / L2)")]
    [SerializeField] private GamepadButton gamepadToggleButton = GamepadButton.LeftTrigger;
    
    [Tooltip("Tombol keyboard alternatif untuk testing (default: Z)")]
    [SerializeField] private Key keyboardToggleKey = Key.Z;

    private bool isActive = false;
    private Vector2 currentScreenPosition;

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        currentScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        SetCursorActive(false);
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
        if (Gamepad.current != null && CheckGamepadButtonPressed(gamepadToggleButton))
        {
            SetCursorActive(!isActive);
            Debug.Log($"[Virtual Cursor] Toggle via Gamepad ({gamepadToggleButton}): {isActive}");
        }

        if (Keyboard.current != null && Keyboard.current[keyboardToggleKey].wasPressedThisFrame)
        {
            SetCursorActive(!isActive);
            Debug.Log($"[Virtual Cursor] Toggle via Keyboard ({keyboardToggleKey}): {isActive}");
        }

        if (!isActive) return;

        Vector2 stickInput = Vector2.zero;
        if (cursorMoveAction != null)
            stickInput = cursorMoveAction.action.ReadValue<Vector2>();
        else if (Gamepad.current != null)
            stickInput = Gamepad.current.leftStick.ReadValue();

        if (stickInput.sqrMagnitude > 0.01f)
        {
            currentScreenPosition = stickInput * (cursorSpeed * Time.unscaledDeltaTime);

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

    public void SetCursorActive(bool active)
    {
        isActive = active;

        if (cursorTransform != null)
            cursorTransform.gameObject.SetActive(active);

        if (active && Mouse.current != null)
        {
            currentScreenPosition = Mouse.current.position.ReadValue();
            UpdateCursorVisualPosition();
        }
    }

    private bool CheckGamepadButtonPressed(GamepadButton button)
    {
        return Gamepad.current[button].wasPressedThisFrame;
    }
}