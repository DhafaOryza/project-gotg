using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Main Panel Reference")]
    [SerializeField] private GameObject _skillTreeMainPanel;

    [Header("Character Profile UI")]
    [SerializeField] private Image _portraitImage;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("Prefabs & Container")]
    [SerializeField] private SkillNodeSlot _skillSlotPrefab;
    [SerializeField] private GameObject _tierHeaderPrefab;
    [SerializeField] private Transform _skillListContentContainer;

    [Header("Detail Panel UI")]
    [SerializeField] private Image _detailIcon;
    [SerializeField] private TextMeshProUGUI _detailNameText;
    [SerializeField] private TextMeshProUGUI _detailDescText;
    [SerializeField] private TextMeshProUGUI _detailCurrentStatText;
    [SerializeField] private TextMeshProUGUI _detailNextStatText;
    [SerializeField] private TextMeshProUGUI _upgradeButtonCostText;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _upgradeButton;

    private List<GameObject> _spawnedUIElements = new List<GameObject>();
    private SkillDataSO _selectedSkillData;
    private SkillNodeSlot _selectedSlot;
    private NPCDataSO _npcData;

    public GameObject SkillTreeMainPanel => _skillTreeMainPanel;
    public Image PortraitImage => _portraitImage;
    public TextMeshProUGUI NameText => _nameText;
    public SkillDataSO SelectedSkillData => _selectedSkillData;
    public Button ExitButton => _exitButton;

    /// <summary>
    /// Initializes the UI components without parameters and retrieves data from GameSessionData.
    /// </summary>
    public void Initialize(NPCDataSO npcData)
    {
        _npcData = npcData;

        if (_skillTreeMainPanel != null)
        {
            _skillTreeMainPanel.SetActive(true);
        }

        SetupButtons();
        GenerateSkillsByTier();

        if (_npcData != null)
        {
            SetupProfile(_npcData.NpcPortrait, _npcData.NpcName);
        }
    }

    /// <summary>
    /// Sets up the character portrait image and name text dynamically.
    /// </summary>
    private void SetupProfile(Sprite portrait, string characterName)
    {
        if (_portraitImage != null)
        {
            _portraitImage.gameObject.SetActive(portrait != null);
            _portraitImage.sprite = portrait;
        }

        if (_nameText != null)
        {
            _nameText.text = characterName;
        }
    }

    /// <summary>
    /// Sets up click listeners for UI buttons.
    /// </summary>
    private void SetupButtons()
    {
        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.RemoveAllListeners();
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.RemoveAllListeners();
            _exitButton.onClick.AddListener(CloseUI);
        }
    }

    /// <summary>
    /// Generates tier headers and skill nodes sequentially from Tier 1 to Tier 4 based on GameSessionData.
    /// </summary>
    public void GenerateSkillsByTier()
    {
        ClearList();

        GameSessionData sessionData = GameSessionData.GetOrCreate();
        if (sessionData == null || sessionData.skills == null || sessionData.skills.Count == 0)
        {
            Debug.LogWarning("[SkillTreeUI] GameSessionData or skill list is empty!");
            return;
        }

        bool isFirstSkillSelected = false;

        for (int currentTier = 1; currentTier <= 4; currentTier++)
        {
            List<SkillDataSO> skillsInTier = sessionData.skills.FindAll(s => s != null && s.tier == currentTier);

            if (skillsInTier.Count == 0) continue;

            if (_tierHeaderPrefab != null)
            {
                GameObject headerObj = Instantiate(_tierHeaderPrefab, _skillListContentContainer);

                TextMeshProUGUI headerText = headerObj.GetComponentInChildren<TextMeshProUGUI>();
                if (headerText != null)
                {
                    headerText.text = $"Tier - {currentTier}";
                }

                _spawnedUIElements.Add(headerObj);
            }

            foreach (var skillData in skillsInTier)
            {
                SkillNodeSlot newSlot = Instantiate(_skillSlotPrefab, _skillListContentContainer);
                newSlot.SetupSlot(skillData, OnSkillSelected);
                _spawnedUIElements.Add(newSlot.gameObject);

                if (!isFirstSkillSelected)
                {
                    OnSkillSelected(skillData, newSlot);
                    isFirstSkillSelected = true;
                }
            }
        }
    }

    /// <summary>
    /// Handles skill selection event and updates UI highlighting and details.
    /// </summary>
    private void OnSkillSelected(SkillDataSO skillData, SkillNodeSlot slot)
    {
        _selectedSlot = slot;
        UpdateDetailPanel(skillData);

        if (_skillListContentContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_skillListContentContainer as RectTransform);
        }
    }

    /// <summary>
    /// Updates the right-side detail panel with the selected skill information.
    /// </summary>
    private void UpdateDetailPanel(SkillDataSO skill)
    {
        if (skill == null) return;

        if (_detailIcon != null && skill.icon != null)
            _detailIcon.sprite = skill.icon;

        if (_detailNameText != null)
            _detailNameText.text = skill.displayName;

        if (_detailDescText != null)
            _detailDescText.text = skill.description;

        if (_detailCurrentStatText != null)
            _detailCurrentStatText.text = $"Base Damage / Val: {skill.baseValue}";

        if (_detailNextStatText != null)
            _detailNextStatText.text = $"Tier: {skill.tier}";

        if (_upgradeButtonCostText != null)
            _upgradeButtonCostText.text = skill.baseValue.ToString();
    }

    /// <summary>
    /// Handles the upgrade button action and processes player economy transactions.
    /// </summary>
    private void OnUpgradeClicked()
    {
        _selectedSkillData = _selectedSlot != null ? _selectedSlot.SkillData : null;
        if (_selectedSkillData == null) return;

        var currencyManager = GameManager.Instance?.currencyManager;

        if (currencyManager.Money >= _selectedSkillData.baseValue)
        {
            currencyManager.SpendMoney(_selectedSkillData.baseValue);
            Debug.Log($"[SkillTreeUI] Successfully upgraded skill: {_selectedSkillData.displayName}");

            UpdateDetailPanel(_selectedSkillData);
        }
        else
        {
            Debug.Log("[SkillTreeUI] Insufficient player coins!");
        }
    }

    /// <summary>
    /// Clears all instantiated UI elements (headers and slots) from the container.
    /// </summary>
    private void ClearList()
    {
        foreach (var obj in _spawnedUIElements)
        {
            if (obj != null) Destroy(obj);
        }
        _spawnedUIElements.Clear();
    }

    /// <summary>
    /// Closes the skill tree panel UI through PlayerUI to ensure active NPC resumes wandering.
    /// </summary>
    public void CloseUI()
    {
        if (_skillTreeMainPanel != null)
        {
            _skillTreeMainPanel.SetActive(false);
        }

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.CloseAllRolePanels();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}