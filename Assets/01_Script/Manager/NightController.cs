using System;
using System.Collections.Generic;
using UnityEngine;

public class NightController : MonoBehaviour
{
    [SerializeField] private int currentNightIndex = 0;
    [SerializeField] private NightWaveDataSO currentNightWave;
    [SerializeField] private bool autoStartNextNight = false; // false = night berikutnya dimulai manual lewat StartNight()

    private WaveController waveController;
    private List<NightWaveDataSO> nightWaves = new List<NightWaveDataSO>();
    private bool isInitialized;

    public event Action<int> OnNightStarted;   // nightIndex
    public event Action<int> OnNightEnded;     // nightIndex
    public event Action OnAllNightsCompleted;

    public int CurrentNightIndex => currentNightIndex;

    public void Initialize()
    {
        if (isInitialized) return;

        var session = GameSessionData.GetOrCreate();
        var registry = session.GetRegistryData();

        if (registry == null || registry.NightWaves == null || registry.NightWaves.Count == 0)
        {
            Debug.LogWarning("[NightController] Registry atau NightWaves kosong.");
            return;
        }

        waveController = GameManager.Instance?.waveController;
        if (waveController == null)
        {
            Debug.LogWarning("[NightController] WaveController belum tersedia di GameManager.");
            return;
        }

        nightWaves = registry.NightWaves;

        if (!SelectNight(currentNightIndex)) return;

        waveController.OnNightCompleted += HandleNightCompleted;
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (waveController != null)
            waveController.OnNightCompleted -= HandleNightCompleted;
    }

    /// <summary>Mulai night yang sedang terpilih (currentNightWave).</summary>
    public void StartNight()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[NightController] Panggil Initialize() dulu sebelum StartNight().");
            return;
        }

        OnNightStarted?.Invoke(currentNightIndex);
        waveController.StartNight(currentNightWave);
        
        var currentPlayer = GameManager.Instance.levelSpawnerManager.GetCurrentPlayer();
        currentPlayer.RedrawSkill();
    }

    private void HandleNightCompleted()
    {
        OnNightEnded?.Invoke(currentNightIndex);
        Debug.Log($"[NightController] Night {currentNightIndex} selesai, menunggu player menekan Continue.");

    }

    public void ProceedToNextNight()
    {
        int nextIndex = currentNightIndex + 1;

        if (nextIndex >= nightWaves.Count)
        {
            Debug.Log("[NightController] Semua night sudah selesai.");
            OnAllNightsCompleted?.Invoke();
            return;
        }

        currentNightIndex = nextIndex;
        SelectNight(currentNightIndex);

        PreparationUI preparationUI = FindAnyObjectByType<PreparationUI>();
        if (preparationUI != null)
            preparationUI.ShowPreparationUI();
        // currentNightIndex = nextIndex;
        // if (!SelectNight(currentNightIndex)) return;

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