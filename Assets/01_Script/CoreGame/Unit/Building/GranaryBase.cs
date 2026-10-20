using UnityEngine;
using UnityEngine.Events;

public class GranaryBase : BuildingBase, IInteractable
{
    [Header("Reference")]
    [SerializeField] private GameObject Sign;

    [Header("Interaction")]
    [Tooltip("Jarak (sumbu X dan Y) dari bangunan di mana sign muncul.")]
    [SerializeField] private float interactRange = 2f;

    public bool isInteractable = true;

    private PlayerBaseEntity player;
    private bool signVisible;

    void Start()
    {
        if (Sign != null) Sign.SetActive(false);
        signVisible = false;
    }

    void Update()
    {
        // Player bisa baru di-spawn atau diganti, jadi dicari ulang kalau belum ada.
        if (player == null)
        {
            player = GameManager.Instance?.levelSpawnerManager.GetCurrentPlayer();
            if (player == null)
            {
                SetSign(false);
                return;
            }
        }

        bool shouldShow = isInteractable && player.CanAct && IsPlayerInRange();
        SetSign(shouldShow);
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

        Sign.SetActive(visible);
        signVisible = visible;
    }

    public void StartNight()
    {
        isInteractable = false;
        SetSign(false);
    }

    public void Interact(PlayerBaseEntity player)
    {
        if (!isInteractable) return;

        // TODO: logika interaksi lumbung (buka UI, ambil makanan, dst.)
        GameManager.Instance?.playerUI.OpenPreparationUI();
        Debug.Log($"[{name}] Interact oleh {player.name}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}