using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Skill Card")]
    [SerializeField] private GameObject skillCardPrefab;
    [SerializeField] private Transform skillContainer;

    private void Start()
    {
        SetupSkillUI();
    }

    private void SetupSkillUI()
    {
        PlayerBaseEntity player = FindObjectOfType<PlayerBaseEntity>();
        if (player == null)
        {
            Debug.LogWarning("[PlayerUI] PlayerBaseEntity tidak ditemukan!");
            return;
        }

        List<SkillDataSO> skills = GameSessionData.GetOrCreate().GetSkillsData();
        if (skills == null && skills.Count == 0)
        {
            Debug.LogWarning("[PlayerUI] List skill di GameSessionData/Registry kosong.");
            return;
        }

        foreach (Transform dummyChild in skillContainer)
        {
            Destroy(dummyChild.gameObject);
        }

        for (int i = 0; i < skills.Count ; i++)
        {
            GameObject cardGO = Instantiate(skillCardPrefab, skillContainer);
            SkillCardUI cardUI = cardGO.GetComponent<SkillCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(player, i, skills[i]);
            }

        }
    }
}