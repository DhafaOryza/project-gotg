using System;
using System.Collections.Generic;
using UnityEngine;

public class NightController : MonoBehaviour
{
    [SerializeField] private int currentNightIndex = 0;
    [SerializeField] private NightWaveDataSO currentNightWave;
    [SerializeField] private bool autoStartNextNight = false;

    private WaveController waveController;
    private List<NightWaveDataSO> nightWaves = new List<NightWaveDataSO>();
    private bool isInitialized;

    // True selama transisi siang -> malam berjalan; mencegah StartNight terpanggil dua kali.
    private bool isStartingNight;

    public int CurrentNightIndex => currentNightIndex;

    public void Initialize()
    {
        if (isInitialized) return;

        var session = GameSessionData.GetOrCreate();

        if (session == null || session.nightwaves == null || session.nightwaves.Count == 0)
        {
            Debug.LogWarning("[NightController] NightWaves Data is Empty on GameSession!");
            return;
        }

        nightWaves = session.nightwaves;

        waveController = GameManager.Instance?.waveController;
        if (waveController == null)
        {
            Debug.LogWarning("[NightController] WaveController belum tersedia di GameManager.");
            return;
        }

        if (!SelectNight(currentNightIndex)) return;

        isInitialized = true;
    }

    private void OnDestroy()
    {
        
    }

    /// <summary>Mulai night yang sedang terpilih (currentNightWave).</summary>
    public void StartNight()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[NightController] Panggil Initialize() dulu sebelum StartNight().");
            return;
        }

        if (isStartingNight) return;

        var gameManager = GameManager.Instance;
        var playerUI = gameManager?.playerUI;
        var spawner = gameManager?.levelSpawnerManager;

        // Langsung (sebelum transisi): tutup panel dan kunci semua interaksi,
        // supaya tidak ada yang bisa diklik atau di-interact selama layar bertransisi.
        playerUI?.ClosePreparationUI();
        spawner?.SetAllInteractable(false);

        var transition = playerUI != null ? playerUI.DayTransition : null;

        if (transition == null)
        {
            // Tanpa DayTransitionUI: jalankan semuanya langsung.
            SwitchWorldToNight();
            BeginWaves();
            return;
        }

        isStartingNight = true;

        bool started = transition.Play(
            $"Night {currentNightIndex + 1}",
            onMidpoint: SwitchWorldToNight,   // layar tertutup penuh: ganti dunia di sini
            onComplete: () =>
            {
                isStartingNight = false;
                BeginWaves();                 // layar sudah terang: baru musuh mulai muncul
            });

        // Transisi menolak (sedang berjalan): jangan biarkan flag tertahan.
        if (!started)
            isStartingNight = false;
    }

    // Dijalankan di pertengahan transisi, saat layar tertutup penuh.
    private void SwitchWorldToNight()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        var currentPlayer = gameManager.levelSpawnerManager?.GetCurrentPlayer();
        if (currentPlayer != null)
            currentPlayer.RedrawSkill();

        var playerUI = gameManager.playerUI;
        if (playerUI != null)
        {
            playerUI.InitializeSkillUI();
            playerUI.OpenSkillUI();
        }

        gameManager.levelSpawnerManager?.DespawnAllNPC();

        gameManager.envController?.SwitchToNight();
    }

    // Dijalankan setelah transisi selesai: wave dimulai saat player sudah bisa melihat layar.
    private void BeginWaves()
    {
        if (waveController == null) return;

        waveController.StartNight(currentNightWave);
    }

    private void HandleNightCompleted()
    {
        Debug.Log($"[NightController] Night {currentNightIndex} selesai, menunggu player menekan Continue.");
    }

    public void ProceedToNextNight()
    {
        int nextIndex = currentNightIndex + 1;

        if (nextIndex >= nightWaves.Count)
        {
            Debug.Log("[NightController] Semua night sudah selesai.");
            return;
        }

        currentNightIndex = nextIndex;
        SelectNight(currentNightIndex);

        if (autoStartNextNight)
            StartNight();
    }

    private bool SelectNight(int index)
    {
        if (index < 0 || index >= nightWaves.Count)
        {
            Debug.LogWarning($"[NightController] Night index {index} tidak valid.");
            return false;
        }

        currentNightWave = nightWaves[index];
        return true;
    }
}