using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PoolManager poolManager;
    public LevelSpawnerManager levelSpawnerManager;

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

        // Initliaze The Manager;
        if (poolManager != null) poolManager.Initialize();
        if (levelSpawnerManager != null) levelSpawnerManager.Initialize();
    }
}