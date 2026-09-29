using System;
using System.Collections;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private LevelSpawnerManager spawner;
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private WaveDataSO waveData; // sumber data wave: timer, interval, dan daftar musuh per wave

    private Coroutine waveRoutine;

    // ---------- Runtime state & events (untuk UI timer / wave manager) ----------
    public bool IsWaveRunning { get; private set; }
    public float WaveTimeRemaining { get; private set; }

    public event Action<int> OnWaveStarted;              // waveIndex
    public event Action<int> OnWaveEnded;                // waveIndex (waktu habis / dihentikan)
    public event Action<float> OnWaveTimeChanged;        // sisa waktu (detik)

    public void Initilaize()
    {
        spawner = GameManager.Instance?.levelSpawnerManager;

        if (waveData == null)
        {
            var session = GameSessionData.GetOrCreate();
            waveData = session.GetWaveData();
        }
    }

    #region Wave Control

    public void StartWave() => StartWave(currentWaveIndex);

    public void StartWave(int waveIndex)
    {
        if (IsWaveRunning)
        {
            Debug.LogWarning("[WaveController] Wave masih berjalan, hentikan dulu dengan StopWave().");
            return;
        }

        if (!IsWaveValid(waveIndex)) return;

        waveRoutine = StartCoroutine(WaveRoutine(waveIndex));
    }

    /// <summary>Hentikan wave sebelum waktunya (mis. semua musuh sudah mati / game over).</summary>
    public void StopWave()
    {
        if (!IsWaveRunning) return;

        if (waveRoutine != null) StopCoroutine(waveRoutine);
        waveRoutine = null;
        IsWaveRunning = false;
    }

    private IEnumerator WaveRoutine(int waveIndex)
    {
        IsWaveRunning = true;
        WaveTimeRemaining = waveData.timerPerWave;
        float spawnTimer = 0f; // 0 -> spawn pertama langsung di awal wave

        OnWaveStarted?.Invoke(waveIndex);
        OnWaveTimeChanged?.Invoke(WaveTimeRemaining);

        while (WaveTimeRemaining > 0f)
        {
            if (spawnTimer <= 0f)
            {
                spawner.SpawnEnemy(waveData.waveEnemies[waveIndex]);
                spawnTimer = GetRandomSpawnInterval(); // jeda berikutnya diacak ulang tiap spawn
            }

            yield return null;

            float dt = Time.deltaTime;
            WaveTimeRemaining = Mathf.Max(0f, WaveTimeRemaining - dt);
            spawnTimer -= dt;

            OnWaveTimeChanged?.Invoke(WaveTimeRemaining);
        }

        IsWaveRunning = false;
        waveRoutine = null;
        currentWaveIndex++; // siap untuk StartWave() berikutnya
        OnWaveEnded?.Invoke(waveIndex);
    }

    private float GetRandomSpawnInterval()
    {
        float[] intervals = waveData != null ? waveData.spawnIntervals : null;

        if (intervals == null || intervals.Length == 0)
            return 3f; // fallback kalau array di WaveData kosong

        float picked = intervals[UnityEngine.Random.Range(0, intervals.Length)];
        return Mathf.Max(0.1f, picked);
    }

    private bool IsWaveValid(int waveIndex)
    {
        if (spawner == null)
        {
            Debug.LogWarning("[WaveController] Referensi LevelSpawnerManager belum di-assign (cek Initilaize() / Inspector).");
            return false;
        }

        if (waveData == null || waveIndex < 0 || waveIndex >= waveData.waveEnemies.Count)
        {
            Debug.LogWarning($"[WaveController] Wave data / wave index {waveIndex} not valid.");
            return false;
        }

        return true;
    }

    #endregion

    #region Public API

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public void SetCurrentWaveIndex(int setWaveIndex)
    {
        currentWaveIndex = setWaveIndex;
    }

    public void SetWaveData(WaveDataSO setWaveData)
    {
        waveData = setWaveData;
    }

    public WaveDataSO GetWaveData()
    {
        return waveData;
    }

    #endregion
}