using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SkillTreeManagerUI : MonoBehaviour
{
    [Header ("Tier Row Configuration")]
    [SerializeField] private List<TierRowData> tierRows = new List<TierRowData>();
    [Header("Prefab & Assets")]
    [SerializeField] private GameObject skillNodePrefab;
    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        RefreshSkillTree();
    }

    public void RefreshSkillTree()
    {
        ClearContainers();
        GenerateAllTiers();
        UpdateCoinDisplay();
    }

    private void ClearContainers()
    {
        foreach (var row in tierRows)
        {
            if (row.skillContainer == null) continue;
            foreach (Transform child in row.skillContainer)
                Destroy(child.gameObject);
        }
    }

    private void ClearContainerChildren(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void GenerateAllTiers()
    {
        if (GameSessionData.Instance == null)
        {
            Debug.LogWarning("[SkillTreeManagerUI] GameSessionData instance tidak ditemukan!");
            return;
        }

        List<SkillDataSO> allskills = GameSessionData.Instance.GeatAllMasterSkills();
        if (allskills == null) return;

        foreach (var skill in allskills)
        {
            if (skill == null) continue;
            Transform targetContainer = null;

            foreach (var row in tierRows)
            {
                if (row.tierlevel == skill.tier)
                {
                    targetContainer = row.skillContainer;
                    break;
                }
            }

            if (targetContainer != null && skillNodePrefab != null)
            {
                GameObject nodeObj = Instantiate(skillNodePrefab, targetContainer);
                SkillTreeNodeCardUI nodeCard = nodeObj.GetComponent<SkillTreeNodeCardUI>();

                if (nodeCard != null)
                    nodeCard.Setup(skill, this);
            }
        }
    }

    private void UpdateCoinDisplay()
    {
        if (coinText.text != null && GameSessionData.Instance != null)
            coinText.text = $"Coins: {GameSessionData.Instance.GetPlayerCoins()}";
    }
}