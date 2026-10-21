using System.Collections.Generic;
using UnityEngine;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Main Panel Reference")]
    [SerializeField] private GameObject _skillTreeMainPanel;

    [Header("Prefab & Dummy Data Source")]
    [SerializeField] private SkillNodeSlot _skillSlotPrefab;
    [SerializeField] private List<SkillSODummy> _allDummySkills;

    [Header("Tier Containers")]
    [SerializeField] private Transform _tier1Container;
    [SerializeField] private Transform _tier2Container;
    [SerializeField] private Transform _tier3Container;
    [SerializeField] private Transform _tier4Container;

    private List<SkillNodeSlot> _spawnedSlots = new List<SkillNodeSlot>();

    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes the Skill Tree UI components and builds the skill nodes using dummy data.
    /// </summary>
    public void Initialize()
    {
        if (_skillTreeMainPanel != null)
        {
            _skillTreeMainPanel.SetActive(true);
        }

        BuildSkillTree();
        Debug.Log("[SkillTreeUI] Initialized with Dummy Data successfully.");
    }

    /// <summary>
    /// Instantiates skill slots dynamically based on SkillSODummy tier configuration.
    /// </summary>
    public void BuildSkillTree()
    {
        // Bersihkan slot lama jika panel dibuka kembali
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();

        if (_skillSlotPrefab == null)
        {
            Debug.LogWarning("[SkillTreeUI] Skill Slot Prefab belum di-assign di Inspector!");
            return;
        }

        if (_allDummySkills == null) return;

        // Loop melalui semua ScriptableObject dummy skill
        foreach (var skillData in _allDummySkills)
        {
            if (skillData == null) continue;

            // Tentukan kontainer tujuan berdasarkan tier skill (1 sampai 4)
            Transform targetContainer = GetTierContainer(skillData.tier);
            if (targetContainer == null) continue;

            // Buat instance dari prefab Skill Slot
            SkillNodeSlot newSlot = Instantiate(_skillSlotPrefab, targetContainer);

            // Hubungkan data visual slot (ikon, harga/baseValue, dan status kunci)
            newSlot.SetupSlot(skillData.icon, skillData.baseValue, skillData.locked);

            _spawnedSlots.Add(newSlot);
        }
    }

    /// <summary>
    /// Helper method to retrieve container transform by tier level.
    /// </summary>
    public Transform GetTierContainer(int tier)
    {
        return tier switch
        {
            1 => _tier1Container,
            2 => _tier2Container,
            3 => _tier3Container,
            4 => _tier4Container,
            _ => _tier1Container
        };
    }

    public void CloseUI()
    {
        if (_skillTreeMainPanel != null)
            _skillTreeMainPanel.SetActive(false);

        gameObject.SetActive(false);
    }
}