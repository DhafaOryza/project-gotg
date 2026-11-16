using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectGamepadScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Kecepatan scroll menggunakan stik kanan")]
    [SerializeField] private float scrollSpeed = 2.5f;

    [Tooltip("Batas mati stik agar tidak scroll sendiri jika analog sedikit drift")]
    [SerializeField] private float deadzone = 0.15f;

    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        if (scrollRect == null) return;

        float rightStickY = 0f;

        // 1. Baca input vertikal dari Right Stick Gamepad aktif
        if (Gamepad.current != null)
        {
            rightStickY = Gamepad.current.rightStick.y.ReadValue();
        }

        // 2. Jika stik kanan digerakkan melebihi deadzone
        if (Mathf.Abs(rightStickY) > deadzone)
        {
            // Tambah/kurang posisi vertikal scroll (1 = Paling Atas, 0 = Paling Bawah)
            scrollRect.verticalNormalizedPosition += rightStickY * scrollSpeed * Time.unscaledDeltaTime;

            // Kunci nilai agar tetap berada di antara 0 dan 1
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }
    }
}