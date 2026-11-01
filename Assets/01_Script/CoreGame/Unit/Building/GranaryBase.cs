using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GranaryBase : BuildingBase, IInteractable
{
    [Header("Reference")]
    [SerializeField] private GameObject Sign;
    [SerializeField] private TextMeshPro txtSign;
    [SerializeField] private TextMeshPro txtBuildingName;

    [Header("Interaction")]
    [Tooltip("Jarak (sumbu X dan Y) dari bangunan di mana sign muncul.")]
    [SerializeField] private float interactRange = 2f;

    public bool isInteractable = true;

    private PlayerBaseEntity player;
    private bool signVisible;

    // Setelah berhasil di-interact, sign disembunyikan dan baru boleh muncul lagi
    // setelah player keluar dari jangkauan lalu mendekat lagi, atau setelah ResumeSign() dipanggil.
    private bool signSuppressed;

    /// <summary>Dipicu saat granary berhasil di-interact. Didengarkan oleh PlayerUI.</summary>
    public event Action OnInteract;

    void Start()
    {
        if (Sign != null) Sign.SetActive(false);
        if (txtBuildingName != null) txtBuildingName.enabled = false;
        signVisible = false;

        UpdateSignText();
        UpdateBuildingName();

        if (player == null)
        {
            player = GameManager.Instance?.levelSpawnerManager.GetCurrentPlayer();
        }

        // Daftarkan diri ke PlayerUI supaya event OnInteract tersambung ke PreparationUI.
        // Dilakukan di Start (bukan Awake) agar PlayerUI.Instance sudah pasti terisi.
        if (PlayerUI.Instance != null)
            PlayerUI.Instance.BindGranary(this);
    }

    void Update()
    {
        if (player == null)
        {
            SetSign(false);
            SetName(false);
            return;
        }

        bool inRange = IsPlayerInRange();

        // Player sudah menjauh: sign boleh muncul lagi pada kedatangan berikutnya.
        if (!inRange)
            signSuppressed = false;

        bool shouldShow = isInteractable && player.CanAct && inRange && !signSuppressed;
        SetSign(shouldShow);
        SetName(shouldShow);
    }

    private bool IsPlayerInRange()
    {
        // Pakai jarak X dan Y saja; sumbu Z diabaikan karena game bergerak di bidang 2D.
        Vector2 diff = (Vector2)player.transform.position - (Vector2)transform.position;
        return diff.sqrMagnitude <= interactRange * interactRange;
    }

    // Hanya memanggil SetActive saat status benar-benar berubah.
    private void SetSign(bool visible)
    {
        if (Sign == null || signVisible == visible) return;

        // Teks tombol diperbarui tepat sebelum sign muncul: aman dari urutan Start antar objek
        // dan ikut berubah kalau tombol interact diganti di Inspector.
        if (visible) UpdateSignText();

        Sign.SetActive(visible);
        signVisible = visible;
    }

    private void UpdateSignText()
    {
        if (txtSign == null) return;

        var playerController = GameManager.Instance?.playerController;
        if (playerController == null) return;

        txtSign.text = playerController.interact.ToString();
    }

    private void SetName(bool visible)
    {
        if (txtBuildingName == null) return;

        if (visible) UpdateBuildingName();

        txtBuildingName.enabled = visible;
    }

    private void UpdateBuildingName()
    {
        if (txtBuildingName == null) return;

        var building = buildingData;
        if (building == null) return;

        var buildingName = building.nameBuilding;
        if (buildingName == null) return;

        txtBuildingName.text = $"{buildingName}";
    }

    // Implementasi IInteractable.SetInteractable: dipakai SetAllInteractable.
    public void SetInteractable(bool value)
    {
        isInteractable = value;

        if (!value)
        {
            SetSign(false);
            SetName(false);
        }
    }

    /// <summary>
    /// Izinkan sign muncul lagi (mis. setelah PreparationUI ditutup). Sign baru benar-benar tampil
    /// kalau player masih di dalam jangkauan dan granary masih interactable.
    /// </summary>
    public void ResumeSign()
    {
        signSuppressed = false;
    }

    public void Interact(PlayerBaseEntity player)
    {
        if (!isInteractable) return;

        // Interaksi berhasil: sembunyikan sign dan tahan sampai PreparationUI ditutup / player menjauh.
        signSuppressed = true;
        SetSign(false);
        SetName(false);

        // "?." wajib: kalau belum ada yang mendengarkan event, Invoke langsung akan NullReferenceException.
        OnInteract?.Invoke();
        Debug.Log($"[{name}] Interact oleh {player.name}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}