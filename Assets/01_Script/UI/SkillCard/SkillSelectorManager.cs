using System.Collections.Generic;
using UnityEngine;

public class SkillSelectorManager : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private Transform skillDeckContainer; 

    [Header("Prefab & Data")]
    [SerializeField] private GameObject skillCardPrefab; 
    [SerializeField] private List<SkillDataSO> availableSkills; 

    private List<SkillSelectorCardUI> spawnedCards = new List<SkillSelectorCardUI>();

    private void OnEnable()
    {
        var registry = Resources.Load<Registry>("TestRegistry");
        if (registry != null)
        {
            availableSkills = registry.defaultSkills;
        }
        GenerateSkillCards();
    }

    public void GenerateSkillCards()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedCards.Clear();

        if (availableSkills == null || availableSkills.Count == 0)
        {
            Debug.LogWarning("Daftar Available Skills masih kosong!");
            return;
        }

        foreach (var skillSO in availableSkills)
        {
            if (skillSO == null) continue;

            GameObject cardObj = Instantiate(skillCardPrefab, contentContainer);
            SkillSelectorCardUI cardUI = cardObj.GetComponent<SkillSelectorCardUI>();

            if (cardUI != null)
            {
                cardUI.Setup(skillSO, contentContainer);
                spawnedCards.Add(cardUI);
            }
        }
    }

    public List<SkillDataSO> GetSelectedDeckSkills()
    {
        List<SkillDataSO> selectedSkills = new List<SkillDataSO>();

        foreach (Transform child in skillDeckContainer)
        {
            SkillSelectorCardUI cardUI = child.GetComponent<SkillSelectorCardUI>();
            if (cardUI != null && cardUI.SkillData != null)
            {
                selectedSkills.Add(cardUI.SkillData);
            }
        }

        return selectedSkills;
    }

    public void SaveandCloseSelector()
    {
        List<SkillDataSO> selectedSkills = GetSelectedDeckSkills();

        if (GameSessionData.Instance != null)
        {
            GameSessionData.Instance.SetSkillsData(selectedSkills);
        }

        PreparationUI preparationUI = FindObjectOfType<PreparationUI>();
        if (preparationUI != null)
        {
            preparationUI.SetSkillSelectorActive(false);
        }
    }
}