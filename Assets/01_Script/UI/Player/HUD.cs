using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private HealthBar playerHealthBar;

    [Header("Granary Health")]
    [SerializeField] private HealthBar granaryHealthBar;

    [Header("Skill HUD")]
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillUI skillUIPrefab;

    [Header("Items HUD")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ItemRadialUI itemRadialUI;

    [Header("Wave Meter")]
    [SerializeField] private WaveMeterUI waveMeter;

    [Header("Notification")]
    [SerializeField] private Notification notification;

    private PlayerBaseEntity currentPlayer;
    private GranaryBase currentGranary;

    public void Initialize()
    {
        if (currentPlayer == null || currentGranary == null)
        {
            var spawnerManager = GameManager.Instance?.levelSpawnerManager;
            currentPlayer = spawnerManager.GetCurrentPlayer();
            currentGranary = spawnerManager.GetCurrentGranary();
        }

        if (playerHealthBar == null ||
            granaryHealthBar == null ||
            skillContainer == null ||
            skillUIPrefab == null ||
            itemContainer == null ||
            itemRadialUI == null ||
            waveMeter == null ||
            notification == null
        )
        {
            Debug.LogError("[HUD] Something References is Null!");
            return;
        }

        if (playerHealthBar != null) playerHealthBar.Initialize(currentPlayer);
        if (granaryHealthBar != null) granaryHealthBar.Initialize(currentGranary);
    }

    public void ActivateHUD() => gameObject.SetActive(true);
    public void DeactivateHUD() => gameObject.SetActive(false);

    public void ActivatePlayerHealthBar() => playerHealthBar.gameObject.SetActive(true);
    public void DeactivatePlayerHealthBar() => granaryHealthBar.gameObject.SetActive(false);

    public void ActivateSkillHUD()
    {
        skillContainer.gameObject.SetActive(true);
        GenerateSkillHUD();
    }
    public void DeactivateSkillHUD() => skillContainer.gameObject.SetActive(false);

    public void ActivateItemsHUD()
    {
        if (itemContainer != null)
            itemContainer.gameObject.SetActive(true);

        if (itemRadialUI == null)
        {
            Debug.LogWarning("ItemRadialUI belum di-assign pada HUD.", this);
            return;
        }

        itemRadialUI.gameObject.SetActive(true);
        itemRadialUI.SyncWithManager();
    }

    public void DeactivateItemsHUD()
    {
        if (itemRadialUI != null)
            itemRadialUI.gameObject.SetActive(false);

        if (itemContainer != null)
            itemContainer.gameObject.SetActive(false);
    }

    public void ActivateWaveMeter() => waveMeter.gameObject.SetActive(true);
    public void DeactivateWaveMeter() => waveMeter.gameObject.SetActive(false);


    #region Helper

    private void GenerateSkillHUD()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        var controller = GameManager.Instance?.playerController;

        if (skillsManager == null || controller == null || skillUIPrefab == null || skillContainer == null)
            return;

        ClearSkillContainer();

        var skills = skillsManager.choosenSkills;
        var keys = new List<string>
        {
          controller.GetSkillDisplayName(0),
          controller.GetSkillDisplayName(1),
          controller.GetSkillDisplayName(2),
          controller.GetSkillDisplayName(3),
        };

        for (int i = 0; i < skills.Count && i < keys.Count; i++)
        {
            SkillUI currentSkill = Instantiate(skillUIPrefab, skillContainer);
            currentSkill.Setup(i, keys[i], skills[i]);
        }

        if (skills.Count > SkillsManager.PassiveSlotIndex)
        {
            var passiveSkillData = skills[SkillsManager.PassiveSlotIndex];
            SkillUI passiveSkillUI = Instantiate(skillUIPrefab, skillContainer);

            passiveSkillUI.Setup(SkillsManager.PassiveSlotIndex, "", passiveSkillData);
        }
    }

    private void ClearSkillContainer()
    {
        if (skillContainer.childCount == 0 || skillContainer.childCount <= 0) return;

        foreach (Transform child in skillContainer)
        {
            Destroy(child);
        }
    }

    #endregion
}
