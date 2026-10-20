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

        waveController.StartNight(currentNightWave);

        var currentPlayer = GameManager.Instance?.levelSpawnerManager.GetCurrentPlayer();
        currentPlayer.RedrawSkill();

        var playerUI = GameManager.Instance?.playerUI;
        playerUI.InitializeSkillUI();
        playerUI.OpenSkillUI();
        
        playerUI.ClosePreparationUI();
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