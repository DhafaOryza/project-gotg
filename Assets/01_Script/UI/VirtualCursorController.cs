using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class VirtualCursorController : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float cursorSpeed = 1200f;

    [Header("Selection Sync")]
    [Tooltip("Kalau ada tombol yang ter-select (mis. lewat navigasi), cursor pindah ke tengah tombol itu.")]
    [SerializeField] private bool moveCursorToSelected = true;
    [Tooltip("Kalau cursor berada di atas tombol, tombol itu otomatis ter-select.")]
    [SerializeField] private bool selectOnCursorHover = true;

    [Header("Input Action")]
    [Tooltip("Action untuk menggerakkan cursor (mis. UI/Virtual Cursor = stick kanan). Kalau kosong, cursor memakai action Navigate dari UI action map.")]
    [SerializeField] private InputActionReference cursorMoveAction;

    private bool isActive;
    private Vector2 currentScreenPosition;
    private InputAction activeCursorAction;

    // Selection yang terakhir kali sudah disinkronkan dengan cursor.
    private GameObject lastSelectedObject;
    private PointerEventData hoverPointerData;
    private EventSystem hoverEventSystem;
    private readonly List<RaycastResult> hoverRaycastResults = new List<RaycastResult>();

    private void Awake()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        currentScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        SetCursorActive(false);
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += HandleDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
        SetCursorActive(false);
    }

    private void Update()
    {
        SyncCursorWithUIInputMode();
        if (!isActive) return;

        BindActiveActionIfNeeded();

        // Tombol ter-select (lewat navigasi, dsb.) -> cursor pindah ke tombol itu.
        SyncCursorToSelection();

        Vector2 stickInput = Vector2.zero;
        if (activeCursorAction != null && activeCursorAction.enabled)
            stickInput = activeCursorAction.ReadValue<Vector2>();
        else if (Gamepad.current != null)
            stickInput = Gamepad.current.rightStick.ReadValue();

        if (stickInput.sqrMagnitude > 0.01f)
        {
            SetCursorScreenPosition(currentScreenPosition + stickInput * (cursorSpeed * Time.unscaledDeltaTime));

            // Cursor berada di atas tombol -> tombol itu otomatis ter-select.
            SyncSelectionToCursor();
        }

    }

    private void SyncCursorWithUIInputMode()
    {
        var playerUI = PlayerUI.Instance;
        var playerController = GameManager.Instance?.playerController;

        bool shouldBeActive = playerUI != null
            && playerUI.IsUIInputActive
            && playerController != null
            && playerController.IsUsingController
            && Gamepad.current != null;

        if (isActive != shouldBeActive)
            SetCursorActive(shouldBeActive);

        if (isActive)
            BindActiveActionIfNeeded();
    }

    private void BindActiveActionIfNeeded()
    {
        // Prioritas: Cursor Move Action dari Inspector (mis. UI/Virtual Cursor = stick kanan).
        // Navigate (D-pad/panah) tidak dipakai menggerakkan cursor kalau action ini terisi,
        // supaya D-pad hanya menavigasi selection dan cursor yang mengikutinya.
        if (cursorMoveAction != null && cursorMoveAction.action != null)
        {
            if (activeCursorAction != cursorMoveAction.action)
                activeCursorAction = cursorMoveAction.action;
            return;
        }

        // Fallback: kalau Cursor Move Action kosong, pakai Navigate dari UI action map.
        var playerUI = PlayerUI.Instance;
        InputAction uiNavigateAction = playerUI != null ? playerUI.UINavigateAction : null;

        if (uiNavigateAction != null && activeCursorAction != uiNavigateAction)
            activeCursorAction = uiNavigateAction;
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
            SyncCursorWithUIInputMode();
    }

    #region Selection Sync

    /// <summary>
    /// Selection -> cursor. Kalau selection berubah (mis. karena navigasi controller),
    /// cursor dipindah ke tengah tombol yang baru ter-select.
    /// </summary>
    private void SyncCursorToSelection()
    {
        var eventSystem = EventSystem.current;
        GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;

        if (selected == lastSelectedObject) return;
        lastSelectedObject = selected;

        if (!moveCursorToSelected || selected == null) return;

        if (TryGetScreenCenter(selected, out Vector2 screenPoint))
            SetCursorScreenPosition(screenPoint);
    }

    /// <summary>
    /// Cursor -> selection. Kalau cursor berada di atas tombol, tombol itu di-select.
    /// </summary>
    private void SyncSelectionToCursor()
    {
        if (!selectOnCursorHover) return;

        var eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        Selectable hovered = GetSelectableUnderCursor(eventSystem);
        if (hovered == null) return;

        GameObject target = hovered.gameObject;
        if (eventSystem.currentSelectedGameObject == target) return;

        // Tandai sudah tersinkron supaya SyncCursorToSelection tidak menarik cursor
        // ke tengah tombol (cursor sudah berada di atasnya).
        lastSelectedObject = target;
        eventSystem.SetSelectedGameObject(target);
    }

    private Selectable GetSelectableUnderCursor(EventSystem eventSystem)
    {
        if (hoverPointerData == null || hoverEventSystem != eventSystem)
        {
            hoverEventSystem = eventSystem;
            hoverPointerData = new PointerEventData(eventSystem);
        }

        hoverPointerData.position = currentScreenPosition;

        hoverRaycastResults.Clear();
        eventSystem.RaycastAll(hoverPointerData, hoverRaycastResults);
        if (hoverRaycastResults.Count == 0) return null;

        // Hasil pertama = elemen UI paling atas di bawah cursor (teks/ikon di dalam tombol ikut terhitung).
        GameObject top = hoverRaycastResults[0].gameObject;
        if (top == null) return null;

        Selectable selectable = top.GetComponentInParent<Selectable>();
        if (selectable == null || !selectable.isActiveAndEnabled || !selectable.IsInteractable())
            return null;

        return selectable;
    }

    private static bool TryGetScreenCenter(GameObject target, out Vector2 screenPoint)
    {
        screenPoint = default;

        var rect = target.transform as RectTransform;
        if (rect == null) return false;

        Camera uiCamera = null;
        Canvas canvas = target.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas = canvas.rootCanvas;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = canvas.worldCamera;
        }

        Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
        screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);
        return true;
    }

    #endregion

    private void SetCursorScreenPosition(Vector2 screenPosition)
    {
        currentScreenPosition = new Vector2(
            Mathf.Clamp(screenPosition.x, 0f, Screen.width),
            Mathf.Clamp(screenPosition.y, 0f, Screen.height));

        UpdateCursorVisualPosition();
        if (Mouse.current != null)
            InputState.Change(Mouse.current.position, currentScreenPosition);
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

        // Reset supaya selection yang sedang aktif langsung disinkronkan ke cursor di Update berikutnya.
        lastSelectedObject = null;

        if (active)
        {
            BindActiveActionIfNeeded();

            if (Mouse.current != null)
                currentScreenPosition = Mouse.current.position.ReadValue();
            else
                currentScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            UpdateCursorVisualPosition();
            if (Mouse.current != null)
                InputState.Change(Mouse.current.position, currentScreenPosition);
        }
        else
        {
            activeCursorAction = null;
        }
    }

}