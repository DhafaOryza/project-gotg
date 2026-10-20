using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [SerializeField] private PreparationUI _preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform _skillContainer;
    [SerializeField] private SkillUI _skillUIPrefab;

    [Header("Conversation UI")]
    [SerializeField] private ConversationUI _conversationUI;

    [Header("Role Specific UIs")]
    [SerializeField] private HealthUI _healthUI;
    [SerializeField] private GranaryUI _granaryUI;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private SkillTreeUI _skillTreeUI;

    private SkillsManager _skillsManager;
    private PlayerController _playerController;

    public ConversationUI ConversationUI => _conversationUI;
    public HealthUI HealthUI => _healthUI;
    public GranaryUI GranaryUI => _granaryUI;
    public ShopUI ShopUI => _shopUI;
    public SkillTreeUI SkillTreeUI => _skillTreeUI;

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

    /// <summary>
    /// Initializes all core user interface components at game start.
    /// </summary>
    public void InitializeUI()
    {
        if (_preparationUI == null) _preparationUI = Object.FindFirstObjectByType<PreparationUI>();
        if (_conversationUI == null) _conversationUI = Object.FindFirstObjectByType<ConversationUI>();
        if (_healthUI == null) _healthUI = Object.FindFirstObjectByType<HealthUI>();
        if (_granaryUI == null) _granaryUI = Object.FindFirstObjectByType<GranaryUI>();
        if (_shopUI == null) _shopUI = Object.FindFirstObjectByType<ShopUI>();
        if (_skillTreeUI == null) _skillTreeUI = Object.FindFirstObjectByType<SkillTreeUI>();

        if (_preparationUI != null) _preparationUI.gameObject.SetActive(false);
        if (_skillContainer != null) _skillContainer.gameObject.SetActive(false);
        if (_healthUI != null) _healthUI.gameObject.SetActive(false);
        if (_granaryUI != null) _granaryUI.gameObject.SetActive(false);
        if (_shopUI != null) _shopUI.gameObject.SetActive(false);
        if (_skillTreeUI != null) _skillTreeUI.gameObject.SetActive(false);
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
    public void ClosePreparationUI() => _preparationUI?.gameObject.SetActive(false);

    public void OpenSkillUI() => _skillContainer?.gameObject.SetActive(true);
    public void CloseSkillUI() => _skillContainer?.gameObject.SetActive(false);
}