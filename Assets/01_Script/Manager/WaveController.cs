using System;
using System.Collections;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [SerializeField] private LevelSpawnerManager spawner;
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private NightWaveDataSO waveData; // data night yang sedang dijalankan

    [Tooltip("Seberapa sering (detik) sisa musuh dicek setelah semua wave selesai.")]
    [SerializeField] private float enemyCheckInterval = 0.5f;

    private Coroutine nightRoutine;

    // Bangunan yang kalau hancur berarti kalah (lumbung). Didengarkan lewat event OnDied.
    private BuildingBase loseTarget;

    // Pengaman supaya ResultUI hanya muncul sekali per night (menang dan kalah bisa saling berdekatan).
    private bool nightEnded;

    // ---------- Runtime state & events ----------
    public bool IsWaveRunning { get; private set; }
    public float WaveTimeRemaining { get; private set; }

    /// <summary>True saat semua wave sudah habis tapi player masih harus membersihkan musuh yang tersisa.</summary>
    public bool IsClearingEnemies { get; private set; }

    /// <summary>Dipicu saat fase "bersihkan sisa musuh" dimulai (mis. untuk menampilkan petunjuk di UI).</summary>
    public event Action OnEnemyClearPhaseStarted;

    private void SubscribeLoseCondition()
    {
        UnsubscribeLoseCondition();

        var granary = GameManager.Instance?.levelSpawnerManager?.GetCurrentGranary();
        if (granary == null)
        {
            Debug.LogWarning("[WaveController] Granary tidak ditemukan, kondisi kalah tidak aktif.");
            return;
        }

        loseTarget = granary;
        loseTarget.OnDied += HandleGranaryDied;
    }

    private void UnsubscribeLoseCondition()
    {
        if (loseTarget != null)
            loseTarget.OnDied -= HandleGranaryDied;

        loseTarget = null;
    }

    private void OnDestroy()
    {
        UnsubscribeLoseCondition();
    }

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

        nightEnded = false;
        IsClearingEnemies = false;
        SubscribeLoseCondition();

        nightRoutine = StartCoroutine(NightRoutine());
    }

    /// <summary>Hentikan night sebelum waktunya (mis. game over).</summary>
    public void StopWave()
    {
        UnsubscribeLoseCondition();
        IsClearingEnemies = false;

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
            // Di-yield langsung (bukan StartCoroutine) supaya StopCoroutine(nightRoutine)
            // ikut menghentikan wave yang sedang berjalan.
            yield return WaveRoutine(currentWaveIndex);
            currentWaveIndex++;
        }

        // Waktu semua wave habis. ResultUI BELUM muncul: player harus membersihkan musuh yang tersisa.
        IsClearingEnemies = true;
        WaveTimeRemaining = 0f;
        OnEnemyClearPhaseStarted?.Invoke();

        var wait = new WaitForSeconds(enemyCheckInterval);
        while (spawner != null && spawner.GetAliveEnemyCount() > 0)
            yield return wait;

        IsClearingEnemies = false;

        // Tidak ada musuh tersisa dan lumbung masih bertahan: menang.
        HandleEndWave(NightResult.Clear);
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

    #region End of Night (Clear / Lose)

    /// <summary>
    /// Satu pintu untuk mengakhiri night, baik menang maupun kalah.
    /// Menghentikan wave yang masih berjalan lalu menampilkan ResultUI sesuai hasilnya.
    /// </summary>
    private void HandleEndWave(NightResult result)
    {
        if (nightEnded) return;
        nightEnded = true;

        UnsubscribeLoseCondition();

        // Kalah di tengah wave / saat membersihkan musuh: hentikan semuanya.
        if (nightRoutine != null)
        {
            StopCoroutine(nightRoutine);
            nightRoutine = null;
        }
        IsWaveRunning = false;
        IsClearingEnemies = false;

        Debug.Log($"[WaveController] Night berakhir: {result}");

        var playerUI = PlayerUI.Instance;
        if (playerUI != null)
            playerUI.OpenResultUI(result);
    }

    // Dipicu BuildingBase.OnDied saat HP lumbung mencapai 0.
    private void HandleGranaryDied()
    {
        HandleEndWave(NightResult.Lose);
    }

    #endregion

    #region Public API

    public int GetCurrentWaveIndex() => currentWaveIndex;
    public void SetCurrentWaveIndex(int setWaveIndex) => currentWaveIndex = setWaveIndex;
    public NightWaveDataSO GetWaveData() => waveData;

    #endregion
}