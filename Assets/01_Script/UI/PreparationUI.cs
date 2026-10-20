using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button btnPreparation;
    [SerializeField] private Button btnExploreWorld;
    [SerializeField] private Button btnStartNight;
    [SerializeField] private TextMeshProUGUI txtNightCounter;

    [Header("Panel")]
    [SerializeField] private GameObject preparationSkillPanel;

    [Header("Prepare Skill UI")]
    [SerializeField] private Button btnPrepareBack;
    [SerializeField] private SkillCard skillCardPrefab;
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SlotUI slotUIPrefab;
    [SerializeField] private Transform skillSlotContainer;

    private NightController nightController;

    void OnEnable()
    {
        Initialize();
        GameManager.Instance.skillsManager.OnChosenSkillsChanged += RedrawPrepareSkill;
    }

    void OnDisable()
    {
        GameManager.Instance.skillsManager.OnChosenSkillsChanged -= RedrawPrepareSkill;
    }

    void Initialize()
    {
        if (nightController == null) nightController = GameManager.Instance?.nightController;

        // Button
        if (btnPreparation != null) btnPreparation.onClick.AddListener(HandlePreparationButton);
        if (btnExploreWorld != null) btnExploreWorld.onClick.AddListener(HandleExploreWorldButton);
        if (btnStartNight != null) btnStartNight.onClick.AddListener(HandleStartNightButton);

        // Night Counter
        HandleNightCounter();

        // Prepare Skill Panel
        if (preparationSkillPanel != null) preparationSkillPanel.SetActive(false);
        if (btnPrepareBack != null) btnPrepareBack.onClick.AddListener(HandleBackPreparationButton);
    }

    private void HandleNightCounter()
    {
        if (txtNightCounter == null || nightController == null) return;

        txtNightCounter.text = $"Night {nightController.CurrentNightIndex + 1}";
    }

    private void HandlePreparationButton()
    {
        preparationSkillPanel.SetActive(true);
        GenerateSkillCard();
        GenerateSkillSlot();
    }

    private void HandleBackPreparationButton()
    {
        preparationSkillPanel.SetActive(false);
    }

    private void HandleExploreWorldButton()
    {
        gameObject.SetActive(false);
    }

    private void HandleStartNightButton()
    {
        nightController.StartNight();
    }

    #region Prepare Skill Panel

    private void GenerateSkillCard()
    {
        var session = GameSessionData.Instance;
        var skills = session.skills;

        var skillsManager = GameManager.Instance?.skillsManager;
        var choosenSkills = skillsManager.choosenSkills;

        ClearSkillContainer();

        foreach (var skill in skills)
        {
            if (skill.locked) continue;

            if (choosenSkills.Contains(skill)) continue;

            SkillCard skillCard = Instantiate(skillCardPrefab, skillContainer);
            skillCard.Initialize(skill);
        }
    }

    private void ClearSkillContainer()
    {
        if (skillContainer.childCount == 0) return;

        for (int i = 0; i < skillContainer.childCount; i++)
        {
            Destroy(skillContainer.GetChild(i).gameObject);
        }
    }

    private void GenerateSkillSlot()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        var playerController = GameManager.Instance?.playerController;

        var keys = new List<KeyCode>
        {
            playerController.skill1,
            playerController.skill2,
            playerController.skill3,
            playerController.skill4,
        };

        ClearSkillSlot();

        var chosen = skillsManager.choosenSkills;

        for (int i = 0; i < skillsManager.maxSkills; i++)
        {
            SlotUI slot = Instantiate(slotUIPrefab, skillSlotContainer);

            string key = i < keys.Count ? keys[i].ToString() : "";
            slot.Initialize(key, skillsManager.GetSkill(i), i);
        }
    }

    private void ClearSkillSlot()
    {
        if (skillSlotContainer.childCount == 0) return;

        for (int i = 0; i < skillSlotContainer.childCount; i++)
        {
            Destroy(skillSlotContainer.GetChild(i).gameObject);
        }
    }

    public void RedrawPrepareSkill()
    {
        GenerateSkillCard();
        GenerateSkillSlot();
    }

    #endregion
}