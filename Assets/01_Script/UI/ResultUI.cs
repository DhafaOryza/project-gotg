using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject clear;
    [SerializeField] private GameObject lose;

    [Header("References")]
    [SerializeField] private Button btnContinue;

    private NightResult lastResult = NightResult.Clear;

    // Listener didaftarkan SEKALI di Awake. Sebelumnya Initialize() dipanggil di Awake dan OnEnable,
    // sehingga tiap ResultUI dibuka listener-nya bertambah (ProceedToNextNight terpanggil berkali-kali).
    void Awake()
    {
        if (btnContinue != null)
            btnContinue.onClick.AddListener(HandleContinueButton);
    }

    void OnDestroy()
    {
        if (btnContinue != null)
            btnContinue.onClick.RemoveListener(HandleContinueButton);
    }

    /// <summary>Tampilkan panel sesuai hasil night.</summary>
    public void Show(NightResult result)
    {
        lastResult = result;

        if (clear != null) clear.SetActive(result == NightResult.Clear);
        if (lose != null) lose.SetActive(result == NightResult.Lose);
    }

    public void ClearNightWave() => Show(NightResult.Clear);

    public void LoseNightWave() => Show(NightResult.Lose);

    // Semua alur setelah Continue diatur GameManager lewat GameState:
    //   menang -> Reset    (night berikutnya, NPC kembali, interactable aktif)
    //   kalah  -> Starting (dunia di-reset: HP penuh, NPC kembali)
    private void HandleContinueButton()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("[ResultUI] GameManager tidak ditemukan.", this);
            return;
        }

        GameState target = lastResult == NightResult.Clear ? GameState.Reset : GameState.Starting;

        // Panel hanya ditutup kalau perpindahan state diterima (mis. ditolak saat transisi masih berjalan).
        if (gameManager.SetState(target))
            gameObject.SetActive(false);
    }
}