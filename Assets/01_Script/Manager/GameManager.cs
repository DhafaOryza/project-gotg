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
        if (poolManager == null) poolManager = Object.FindFirstObjectByType<PoolManager>();
        if (levelSpawnerManager == null) levelSpawnerManager = Object.FindFirstObjectByType<LevelSpawnerManager>();
        if (nightController == null) nightController = Object.FindFirstObjectByType<NightController>();
        if (waveController == null) waveController = Object.FindFirstObjectByType<WaveController>();
        if (questManager == null) questManager = Object.FindFirstObjectByType<QuestManager>();
        if (playerController == null) playerController = Object.FindFirstObjectByType<PlayerController>();
        if (skillsManager == null) skillsManager = Object.FindFirstObjectByType<SkillsManager>();
        if (cinemachine == null) cinemachine = Object.FindFirstObjectByType<CinemachineCamera>();
        if (playerUI == null) playerUI = Object.FindFirstObjectByType<PlayerUI>();
        if (envController == null) envController = Object.FindFirstObjectByType<EnvController>();

        // Initliaze The Manager;
        if (poolManager != null) poolManager.Initialize();
        if (levelSpawnerManager != null) levelSpawnerManager.Initialize();
        if (nightController != null) nightController.Initialize();
        if (questManager != null) questManager.Initialize();
        if (playerController != null) playerController.Setup();
    }
}