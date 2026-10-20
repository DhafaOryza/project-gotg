using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [SerializeField] private PreparationUI _preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform _skillContainer;
    [SerializeField] private SkillUI _skillUIPrefab;

    [Header("Dialog System")]
    [SerializeField] private ConversationUI _conversationUI;

    private SkillsManager _skillsManager;
    private PlayerController _playerController;

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

    /// <summary>
    /// Initializes all core user interface components at game start.
    /// </summary>
    public void InitializeUI()
    {
        if (_preparationUI == null) _preparationUI = Object.FindFirstObjectByType<PreparationUI>();
        if (_conversationUI == null) _conversationUI = Object.FindFirstObjectByType<ConversationUI>();

        if (_preparationUI != null) _preparationUI.gameObject.SetActive(false);
        if (_skillContainer != null) _skillContainer.gameObject.SetActive(false);
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
            currentSkill.Setup(skills[i].icon, keys[i].ToString());
        }
    }

    public void OpenPreparationUI() => _preparationUI?.gameObject.SetActive(true);
    public void ClosePreparationUI() => _preparationUI?.gameObject.SetActive(false);

    public void OpenSkillUI() => _skillContainer?.gameObject.SetActive(true);
    public void CloseSkillUI() => _skillContainer?.gameObject.SetActive(false);
}