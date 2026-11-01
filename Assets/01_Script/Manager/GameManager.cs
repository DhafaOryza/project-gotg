using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PoolManager poolManager;
    public LevelSpawnerManager levelSpawnerManager;
    public NightController nightController;
    public WaveController waveController;
    public QuestManager questManager;
    public PlayerController playerController;
    public SkillsManager skillsManager;
    public CinemachineCamera cinemachine;
    public PlayerUI playerUI;
    public EnvController envController;
    public ConsumableAndItemManager consumableAndItemManager;
    public CurrencyManager currencyManager;

    [Header("Game Flow")]
    [SerializeField] private GameState currentState = GameState.Starting;

    [Tooltip("Setelah kalah: true = mulai lagi dari Night 1, false = ulangi night yang sama.")]
    [SerializeField] private bool restartFromFirstNightOnLose = false;

    /// <summary>State saat ini. Ubah lewat SetState() supaya alur yang menyertainya ikut dijalankan.</summary>
    public GameState gameState => currentState;

    /// <summary>True selama transisi layar berjalan; perpindahan state ditolak selama itu.</summary>
    public bool IsTransitioning { get; private set; }

    public event System.Action<GameState> OnGameStateChanged;

    private bool hasInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (hasInitialized) return;
        InitilaizeManagers();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasInitialized = false;
        InitilaizeManagers();
    }

    #region Initialize (GameState.Starting saat pertama kali)

    private void InitilaizeManagers()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        // Awal permainan / scene baru selalu dimulai dari Starting.
        currentState = GameState.Starting;
        IsTransitioning = false;

        // Validation Object Manager
        if (poolManager == null) poolManager = Object.FindAnyObjectByType<PoolManager>();
        if (levelSpawnerManager == null) levelSpawnerManager = Object.FindAnyObjectByType<LevelSpawnerManager>();
        if (nightController == null) nightController = Object.FindAnyObjectByType<NightController>();
        if (waveController == null) waveController = Object.FindFirstObjectByType<WaveController>();
        if (questManager == null) questManager = Object.FindAnyObjectByType<QuestManager>();
        if (playerController == null) playerController = Object.FindFirstObjectByType<PlayerController>();
        if (skillsManager == null) skillsManager = Object.FindFirstObjectByType<SkillsManager>();
        if (cinemachine == null) cinemachine = Object.FindAnyObjectByType<CinemachineCamera>();
        if (playerUI == null) playerUI = Object.FindAnyObjectByType<PlayerUI>();
        if (envController == null) envController = Object.FindAnyObjectByType<EnvController>();
        if (consumableAndItemManager == null) consumableAndItemManager = Object.FindAnyObjectByType<ConsumableAndItemManager>();
        if (currencyManager == null) currencyManager = Object.FindFirstObjectByType<CurrencyManager>();

        // Initliaze The Manager; (spawn player, lumbung, dan NPC dengan HP penuh)
        if (poolManager != null) poolManager.Initialize();
        if (levelSpawnerManager != null) levelSpawnerManager.Initialize();
        if (nightController != null) nightController.Initialize();
        if (questManager != null) questManager.Initialize();
        if (playerController != null) playerController.Setup();
    }

    #endregion

    #region State Machine

    /// <summary>
    /// Pindah ke state baru dan jalankan alur yang menyertainya.
    /// Return false kalau perpindahan ditolak (transisi sedang berjalan, state tidak valid, atau manager belum siap).
    ///   Starting -> Combat : player menekan Start Night
    ///   Combat   -> Reset  : menang, player menekan Continue
    ///   Combat   -> Starting : kalah, player menekan Continue
    ///   Reset    -> Combat : Start Night di hari berikutnya
    /// </summary>
    public bool SetState(GameState next)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning($"[GameManager] Transisi sedang berjalan, perpindahan ke {next} diabaikan.");
            return false;
        }

        if (!IsValidTransition(currentState, next))
        {
            Debug.LogWarning($"[GameManager] Perpindahan state tidak valid: {currentState} -> {next}.");
            return false;
        }

        if (!CanEnter(next)) return false;

        var previous = currentState;
        currentState = next;
        Debug.Log($"[GameManager] State: {previous} -> {next}");
        OnGameStateChanged?.Invoke(next);

        switch (next)
        {
            case GameState.Combat: EnterCombat(); break;
            case GameState.Reset: EnterReset(); break;
            case GameState.Starting: EnterStarting(); break;
        }

        return true;
    }

    private static bool IsValidTransition(GameState from, GameState to)
    {
        switch (from)
        {
            case GameState.Starting: return to == GameState.Combat;
            case GameState.Combat: return to == GameState.Reset || to == GameState.Starting;
            case GameState.Reset: return to == GameState.Combat;
            default: return false;
        }
    }

    private bool CanEnter(GameState next)
    {
        if (levelSpawnerManager == null || nightController == null)
        {
            Debug.LogError("[GameManager] levelSpawnerManager atau nightController belum tersedia.");
            return false;
        }

        if (next == GameState.Combat && !nightController.IsInitialized)
        {
            Debug.LogWarning("[GameManager] NightController belum di-Initialize, Combat tidak bisa dimulai.");
            return false;
        }

        return true;
    }

    #endregion

    #region Combat (siang -> malam)

    private void EnterCombat()
    {
        // Langsung (sebelum transisi): tutup panel dan kunci semua interaksi.
        playerUI?.ClosePreparationUI();
        levelSpawnerManager.SetAllInteractable(false);

        RunTransition(
            DayTransitionType.Night,
            $"Night {nightController.CurrentNightIndex + 1}",
            onMidpoint: SwitchWorldToNight,    // layar tertutup penuh
            onComplete: nightController.BeginWaves);   // layar terang: musuh mulai muncul
    }

    private void SwitchWorldToNight()
    {
        var player = levelSpawnerManager.GetCurrentPlayer();
        if (player != null)
            player.RedrawSkill();

        if (playerUI != null)
        {
            playerUI.InitializeSkillUI();
            playerUI.OpenSkillUI();
            playerUI.OpenConsumableUI();
        }

        levelSpawnerManager.DespawnAllNPC();
        envController?.SwitchToNight();
    }

    #endregion

    #region Reset (menang -> pagi)

    private void EnterReset()
    {
        // Naik ke night berikutnya (label "Day N" memakai indeks yang sudah diperbarui).
        if (!nightController.ProceedToNextNight())
            Debug.Log("[GameManager] Semua night sudah selesai. TODO: tampilan tamat / menu utama.");

        RunTransition(
            DayTransitionType.Day,
            $"Day {nightController.CurrentNightIndex + 1}",
            onMidpoint: SwitchWorldToDay,
            onComplete: UnlockInteractions);
    }

    private void SwitchWorldToDay()
    {
        playerUI?.CloseSkillUI();
        playerUI?.CloseConsumableUI();

        levelSpawnerManager.DespawnAllEnemy();
        levelSpawnerManager.SpawnNPCs();

        envController?.SwitchToDay();
    }

    #endregion

    #region Starting (setelah kalah)

    // Dipanggil saat kembali ke Starting setelah kalah (saat pertama kali, Starting ditangani InitilaizeManagers).
    private void EnterStarting()
    {
        if (restartFromFirstNightOnLose)
            nightController.ResetProgress();

        // Pastikan wave benar-benar berhenti (biasanya sudah berhenti saat lumbung hancur).
        waveController?.StopWave();

        RunTransition(
            DayTransitionType.Day,
            $"Day {nightController.CurrentNightIndex + 1}",
            onMidpoint: RestartWorld,
            onComplete: UnlockInteractions);
    }

    private void RestartWorld()
    {
        playerUI?.CloseSkillUI();
        playerUI?.CloseConsumableUI();

        levelSpawnerManager.DespawnAllEnemy();
        levelSpawnerManager.ResetCharacters();    // player dan lumbung kembali dengan HP penuh
        levelSpawnerManager.SpawnNPCs();

        playerController?.Stop();
        envController?.SwitchToDay();
    }

    #endregion

    #region Helpers

    private void UnlockInteractions()
    {
        levelSpawnerManager?.SetAllInteractable(true);
    }

    // Jalankan transisi layar; kalau DayTransitionUI tidak ada, langkahnya dijalankan langsung.
    private void RunTransition(DayTransitionType type, string label, System.Action onMidpoint, System.Action onComplete)
    {
        var transition = playerUI != null ? playerUI.DayTransition : null;

        if (transition == null)
        {
            onMidpoint?.Invoke();
            onComplete?.Invoke();
            return;
        }

        IsTransitioning = true;

        bool started = transition.Play(type, label, onMidpoint, () =>
        {
            IsTransitioning = false;
            onComplete?.Invoke();
        });

        // Transisi lain sedang berjalan: jangan sampai alur macet, jalankan langsung.
        if (!started)
        {
            IsTransitioning = false;
            onMidpoint?.Invoke();
            onComplete?.Invoke();
        }
    }

    #endregion
}