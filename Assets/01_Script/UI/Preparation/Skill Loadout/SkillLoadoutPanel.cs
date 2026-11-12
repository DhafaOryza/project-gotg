using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillLoadoutPanel : MonoBehaviour
{
    [Header("References")]
    [Header("Skill Card")]
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillCardUI skillCardPrefab;

    [Header("Skill Slot")]
    [SerializeField] private Transform skillSlotContainer;
    [SerializeField] private SkillSlotUI skillSlotPrefab;

    [Header("Detail Skill")]
    [SerializeField] private GameObject detailContainer;
    [SerializeField] private TextMeshProUGUI txtSkillname;
    [SerializeField] private Image imgSkillIcon;
    [SerializeField] private TextMeshProUGUI txtSkillLevel;
    [SerializeField] private TextMeshProUGUI txtSkillDesc;

    private SkillDataSO selectedSkill;

    private static readonly string[] DefaultSkillKeys = { "Q", "W", "E", "R" };

    private void OnEnable()
    {
        SubscribeToSkillsManager();
        Initialize();
    }

    private void OnDisable()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
        {
            skillsManager.OnChosenSkillsChanged -= GenerateSkillCard;
            skillsManager.OnChosenSkillsChanged -= GenerateSkillSlot;
        }
    }

    public void Initialize()
    {
        GenerateSkillCard();
        GenerateSkillSlot();
    }

    #region Helper

    private void GenerateSkillCard()
    {
        var session = GameSessionData.GetOrCreate();

        if (session == null || skillContainer == null || skillCardPrefab == null) return;

        ClearContainer(skillContainer);
        var skills = session.skills;
        if (skills == null) return;

        var chosenSkills = GameManager.Instance?.skillsManager?.choosenSkills;

        foreach (var skill in skills)
        {
            if (skill == null || skill.IsLocked) continue;
            if (chosenSkills != null && chosenSkills.Contains(skill)) continue;

            SkillCardUI skillCard = Instantiate(skillCardPrefab, skillContainer);
            skillCard.Initialize(skill);
        }
    }

    public void GenerateSkillSlot()
    {
        if (skillSlotContainer == null || skillSlotPrefab == null) return;

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null) return;

        ClearContainer(skillSlotContainer);
        var playerController = GameManager.Instance?.playerController;

        for (int i = 0; i < skillsManager.maxSkills; i++)
        {
            SkillSlotUI skillSlot = Instantiate(skillSlotPrefab, skillSlotContainer);
            string keyBind = i < DefaultSkillKeys.Length
                ? (playerController != null ? playerController.GetSkillDisplayName(i) : DefaultSkillKeys[i])
                : string.Empty;

            skillSlot.Initialize(keyBind, skillsManager.GetSkill(i), i);
        }
    }

    private void SubscribeToSkillsManager()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null) return;

        skillsManager.OnChosenSkillsChanged -= GenerateSkillCard;
        skillsManager.OnChosenSkillsChanged += GenerateSkillCard;
        skillsManager.OnChosenSkillsChanged -= GenerateSkillSlot;
        skillsManager.OnChosenSkillsChanged += GenerateSkillSlot;
    }

    private static void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    #endregion   
}
