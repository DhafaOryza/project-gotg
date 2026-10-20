using System;
using System.Collections;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private LevelSpawnerManager spawner;
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private NightWaveDataSO waveData; // data night yang sedang dijalankan

    private Coroutine nightRoutine;

    // ---------- Runtime state & events ----------
    public bool IsWaveRunning { get; private set; }
    public float WaveTimeRemaining { get; private set; }

    #region Night / Wave Control

    /// <summary>Dipanggil NightController. Menjalankan seluruh wave dalam satu night.</summary>
    public void StartNight(NightWaveDataSO nightData)
    {
        if (IsWaveRunning)
        {
            Debug.LogWarning("[WaveController] Night masih berjalan, hentikan dulu dengan StopWave().");
            return;
        }

        spawner = GameManager.Instance?.levelSpawnerManager;
        waveData = nightData;
        currentWaveIndex = 0; // night baru selalu mulai dari wave pertama

        if (!IsNightValid()) return;

        nightRoutine = StartCoroutine(NightRoutine());
    }

    /// <summary>Hentikan night sebelum waktunya (mis. game over).</summary>
    public void StopWave()
    {
        if (!IsWaveRunning) return;

        if (nightRoutine != null) StopCoroutine(nightRoutine);
        nightRoutine = null;
        IsWaveRunning = false;
    }

    private IEnumerator NightRoutine()
    {
        IsWaveRunning = true;

        // jalankan wave satu per satu sampai waveEnemies habis
        while (currentWaveIndex < waveData.waveEnemies.Count)
        {
            yield return StartCoroutine(WaveRoutine(currentWaveIndex));
            currentWaveIndex++;
        }

        IsWaveRunning = false;
        nightRoutine = null;
    }

    private IEnumerator WaveRoutine(int waveIndex)
    {
        WaveTimeRemaining = waveData.timerPerWave;
        float spawnTimer = 0f; // 0 -> spawn pertama langsung di awal wave

        while (WaveTimeRemaining > 0f)
        {
            if (spawnTimer <= 0f)
            {
                spawner.SpawnEnemy(waveData.waveEnemies[waveIndex]);
                spawnTimer = GetRandomSpawnInterval();
            }

            yield return null;

            float dt = Time.deltaTime;
            WaveTimeRemaining = Mathf.Max(0f, WaveTimeRemaining - dt);
            spawnTimer -= dt;

        }
    }

    private float GetRandomSpawnInterval()
    {
        float[] intervals = waveData != null ? waveData.spawnIntervals : null;

        if (intervals == null || intervals.Length == 0)
            return 3f;

        float picked = intervals[UnityEngine.Random.Range(0, intervals.Length)];
        return Mathf.Max(0.1f, picked);
    }

    private bool IsNightValid()
    {
        if (spawner == null)
        {
            Debug.LogWarning("[WaveController] Referensi LevelSpawnerManager belum di-assign.");
            return false;
        }

        if (waveData == null || waveData.waveEnemies == null || waveData.waveEnemies.Count == 0)
        {
            Debug.LogWarning("[WaveController] NightWaveData kosong atau tidak valid.");
            return false;
        }

        return true;
    }

    #endregion

    #region Public API

    public int GetCurrentWaveIndex() => currentWaveIndex;
    public void SetCurrentWaveIndex(int setWaveIndex) => currentWaveIndex = setWaveIndex;
    public NightWaveDataSO GetWaveData() => waveData;

    #endregion
}