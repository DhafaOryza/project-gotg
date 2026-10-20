using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [SerializeField] private PreparationUI preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillUI skillUIPrefab;

    private SkillsManager skillsManager;
    private PlayerController playerController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            return;
        }
    }

    void Start()
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        if (preparationUI == null) preparationUI = Object.FindFirstObjectByType<PreparationUI>();

        if (preparationUI != null) preparationUI.gameObject.SetActive(false);
        if (skillContainer != null) skillContainer.gameObject.SetActive(false);
    }

    public void InitializeSkillUI()
    {
        skillsManager = GameManager.Instance?.skillsManager;
        playerController = GameManager.Instance?.playerController;

        if (skillsManager == null || playerController == null || skillUIPrefab == null || skillContainer == null)
            return;

        var skills = skillsManager.choosenSkills;
        var keys = new List<KeyCode>
        {
            playerController.skill1,
            playerController.skill2,
            playerController.skill3,
            playerController.skill4,
        };

        for (int i = 0; i < skills.Count && i < keys.Count; i++)
        {
            SkillUI currentSkill = Instantiate(skillUIPrefab, skillContainer);
            currentSkill.Setup(skills[i].icon, keys[i].ToString());
        }
    }

    public void OpenPreparationUI()
    {
        preparationUI.gameObject.SetActive(true);
    }

    public void ClosePreparationUI()
    {
        preparationUI.gameObject.SetActive(false);
    }

    public void OpenSkillUI()
    {
        skillContainer.gameObject.SetActive(true);
    }

    public void CloseSkillUI()
    {
        skillContainer.gameObject.SetActive(false);
    }
}