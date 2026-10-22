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

    private void InitilaizeManagers()
    {
        if (hasInitialized) return;
        hasInitialized = true;

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

        // Initliaze The Manager;
        if (poolManager != null) poolManager.Initialize();
        if (levelSpawnerManager != null) levelSpawnerManager.Initialize();
        if (nightController != null) nightController.Initialize();
        if (questManager != null) questManager.Initialize();
        if (playerController != null) playerController.Setup();
    }
}