using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [Header("Player Profile")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar granaryHealthBar;

    [Header("Preparation")]
    [SerializeField] private PreparationUI _preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform _skillContainer;
    [SerializeField] private SkillUI _skillUIPrefab;

    [Header("Dialog System")]
    [SerializeField] private ConversationUI _conversationUI;

    private SkillsManager _skillsManager;
    private PlayerController _playerController;
    private PlayerBaseEntity player;
    private GranaryBase _granary;

    public ConversationUI DialogSystem => _conversationUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeUI();
    }

    void OnEnable()
    {
        if (_granary != null)
            _granary.OnInteract += OpenPreparationUI;
    }

    void OnDisable()
    {
        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;
    }

    /// <summary>
    /// Sambungkan event OnInteract milik granary ke PreparationUI.
    /// Dipanggil oleh GranaryBase saat Start (atau oleh PlayerUI.Start untuk granary yang sudah ada).
    /// </summary>
    public void BindGranary(GranaryBase granary)
    {
        if (_granary == granary) return;

        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;

        _granary = granary;

        if (_granary != null && isActiveAndEnabled)
            _granary.OnInteract += OpenPreparationUI;
    }

    /// <summary>
    /// Initializes all core user interface components at game start.
    /// </summary>
    public void InitializeUI()
    {
        if (_preparationUI == null) _preparationUI = Object.FindFirstObjectByType<PreparationUI>();
        if (_conversationUI == null) _conversationUI = Object.FindFirstObjectByType<ConversationUI>();

        if (_preparationUI != null) _preparationUI.gameObject.SetActive(false);
        if (_skillContainer != null) _skillContainer.gameObject.SetActive(false);

        if (_granary == null)
        {
            var granary = GameManager.Instance?.levelSpawnerManager?.GetCurrentGranary();
            if (granary != null) BindGranary(granary);
        }

        if (player == null)
        {
            var currentPlayer = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
            player = currentPlayer;
        }

        InitializeHealthBar();
    }

    public void InitializeHealthBar()
    {
        playerHealthBar.Initialize(player);
        granaryHealthBar.Initialize(_granary);
    }

    public void InitializeSkillUI()
    {
        _skillsManager = GameManager.Instance?.skillsManager;
        _playerController = GameManager.Instance?.playerController;

        if (_skillsManager == null || _playerController == null || _skillUIPrefab == null || _skillContainer == null)
            return;

        var skills = _skillsManager.choosenSkills;
        var keys = new List<KeyCode>
        {
            _playerController.skill1,
            _playerController.skill2,
            _playerController.skill3,
            _playerController.skill4,
        };

        for (int i = 0; i < skills.Count && i < keys.Count; i++)
        {
            SkillUI currentSkill = Instantiate(_skillUIPrefab, _skillContainer);
            currentSkill.Setup(keys[i].ToString(), skills[i]);
        }
    }

    public void OpenPreparationUI() => _preparationUI?.gameObject.SetActive(true);

    public void ClosePreparationUI()
    {
        _preparationUI?.gameObject.SetActive(false);

        if (_granary != null)
            _granary.ResumeSign();
    }

    public void OpenSkillUI() => _skillContainer?.gameObject.SetActive(true);
    public void CloseSkillUI() => _skillContainer?.gameObject.SetActive(false);
}