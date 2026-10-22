using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menyimpan data night (urutan, night yang sedang terpilih) dan memulai wave.
/// Alur siang/malam (transisi, NPC, env, interactable) sekarang diatur GameManager lewat GameState.
/// </summary>
public class NightController : MonoBehaviour
{
    [SerializeField] private int currentNightIndex = 0;
    [SerializeField] private NightWaveDataSO currentNightWave;

    private WaveController waveController;
    private List<NightWaveDataSO> nightWaves = new List<NightWaveDataSO>();
    private bool isInitialized;

    public int CurrentNightIndex => currentNightIndex;
    public NightWaveDataSO CurrentNightWave => currentNightWave;
    public bool IsInitialized => isInitialized;
    public bool HasNextNight => currentNightIndex + 1 < nightWaves.Count;

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

    /// <summary>
    /// Pintu masuk lama (mis. dari PreparationUI). Sekarang hanya meneruskan ke state machine:
    /// GameManager.SetState(Combat) yang menjalankan seluruh alurnya.
    /// </summary>
    public void StartNight()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[NightController] Panggil Initialize() dulu sebelum StartNight().");
            return;
        }

        GameManager.Instance?.SetState(GameState.Combat);
    }

    /// <summary>Dipanggil GameManager setelah transisi selesai: wave dimulai saat layar sudah terang.</summary>
    public void BeginWaves()
    {
        if (waveController == null)
            waveController = GameManager.Instance?.waveController;

        if (waveController == null)
        {
            Debug.LogWarning("[NightController] WaveController tidak ditemukan, wave tidak dimulai.");
            return;
        }

        waveController.StartNight(currentNightWave);
    }

    /// <summary>
    /// Naik ke night berikutnya. Return false kalau sudah di night terakhir (indeks tidak berubah).
    /// </summary>
    public bool ProceedToNextNight()
    {
        if (!HasNextNight)
        {
            Debug.Log("[NightController] Semua night sudah selesai.");
            return false;
        }

        currentNightIndex++;
        SelectNight(currentNightIndex);
        return true;
    }

    /// <summary>Kembali ke night pertama (dipakai saat kalah kalau restartFromFirstNightOnLose aktif).</summary>
    public void ResetProgress()
    {
        currentNightIndex = 0;
        SelectNight(currentNightIndex);
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