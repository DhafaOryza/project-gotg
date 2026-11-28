using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour
{
    [Header("Main Panel Reference")]
    [SerializeField] private GameObject _skillTreeMainPanel;

    [Header("Character Profile UI")]
    [SerializeField] private Image _portraitImage;

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

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _currencyText;

    [Header("UI Controls")]
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _upgradeButton;

    [Header("Navigation")]
    [SerializeField] private Selectable firstSelectedButton;

    public Selectable FirstSelectedButton => firstSelectedButton;

    private readonly List<GameObject> _spawnedUIElements = new List<GameObject>();
    private readonly List<SkillNodeSlot> _spawnedSlots = new List<SkillNodeSlot>();

    private SkillDataSO _selectedSkillData;
    private SkillNodeSlot _selectedSlot;
    private NPCDataSO _npcData;
    private CurrencyManager _currencyManager;

    public GameObject SkillTreeMainPanel => _skillTreeMainPanel;

    /// <summary>
    /// Initializes the UI. Called by PlayerUI when the NPC panel opens.
    /// </summary>
    public void Initialize(NPCDataSO npcData)
    {
        _npcData = npcData;

        if (_skillTreeMainPanel != null)
            _skillTreeMainPanel.SetActive(true);

        // Subscribe to currency changes so all slot buttons update in real time.
        UnsubscribeCurrency();
        _currencyManager = GameManager.Instance?.currencyManager;
        if (_currencyManager != null)
            _currencyManager.OnChange += HandleCurrencyChanged;

        SetupButtons();
        GenerateSkillsByTier();
        RefreshCurrencyText();

        if (_npcData != null)
            SetupProfile(_npcData.NpcPortrait);

        PlayerUI.Instance?.SetSelectedObject(firstSelectedButton);
    }

    private void OnDisable()
    {
        UnsubscribeCurrency();
    }

    private void UnsubscribeCurrency()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= HandleCurrencyChanged;
    }

    // Runs every time the player's money changes (purchase, reward, etc.).
    private void HandleCurrencyChanged()
    {
        RefreshCurrencyText();
        RefreshAllSlots();
    }

    private void RefreshCurrencyText()
    {
        if (_currencyText == null) return;

        int money = _currencyManager != null ? _currencyManager.Money : 0;
        _currencyText.text = $"{money}";
    }

    // ── Slot refresh ─────────────────────────────────────────────────────

    private void RefreshAllSlots()
    {
        foreach (var slot in _spawnedSlots)
        {
            if (slot != null) slot.Refresh();
        }
    }

    // ── Profile ───────────────────────────────────────────────────────────

    private void SetupProfile(Sprite portrait)
    {
        if (_portraitImage != null)
        {
            _portraitImage.gameObject.SetActive(portrait != null);
            _portraitImage.sprite = portrait;
        }
    }

    // ── Buttons ───────────────────────────────────────────────────────────

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

    // ── Generation ────────────────────────────────────────────────────────

    /// <summary>
    /// Generates tier headers and skill nodes from GameSessionData.
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

        for (int tier = 1; tier <= 4; tier++)
        {
            var skillsInTier = sessionData.skills.FindAll(s => s != null && s.tier == tier);
            if (skillsInTier.Count == 0) continue;

            if (_tierHeaderPrefab != null)
            {
                GameObject headerObj = Instantiate(_tierHeaderPrefab, _skillListContentContainer);
                var headerText = headerObj.GetComponentInChildren<TextMeshProUGUI>();
                if (headerText != null) headerText.text = $"Tier - {tier}";
                _spawnedUIElements.Add(headerObj);
            }

            foreach (var skillData in skillsInTier)
            {
                SkillNodeSlot newSlot = Instantiate(_skillSlotPrefab, _skillListContentContainer);
                newSlot.SetupSlot(skillData, OnSkillSelected);
                _spawnedUIElements.Add(newSlot.gameObject);
                _spawnedSlots.Add(newSlot);

                if (!isFirstSkillSelected)
                {
                    OnSkillSelected(skillData, newSlot);
                    isFirstSkillSelected = true;
                }
            }
        }
    }

    // ── Selection ─────────────────────────────────────────────────────────

    private void OnSkillSelected(SkillDataSO skillData, SkillNodeSlot slot)
    {
        _selectedSlot = slot;
        _selectedSkillData = skillData;
        UpdateDetailPanel(skillData);
        RefreshUpgradeButton();

        if (_skillListContentContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(_skillListContentContainer as RectTransform);
    }

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
            _upgradeButtonCostText.text = skill.isPurchased ? "PURCHASED" : skill.price.ToString();
    }

    private void RefreshUpgradeButton()
    {
        if (_upgradeButton == null || _selectedSkillData == null) return;

        bool canAfford = _currencyManager != null
                         && _currencyManager.Money >= _selectedSkillData.price;

        _upgradeButton.interactable = !_selectedSkillData.isPurchased
                                      && !_selectedSkillData.locked
                                      && canAfford;
    }

    // ── Purchase ──────────────────────────────────────────────────────────

    private void OnUpgradeClicked()
    {
        if (_selectedSkillData == null || _currencyManager == null) return;

        if (_selectedSkillData.isPurchased)
        {
            Debug.Log($"[SkillTreeUI] {_selectedSkillData.displayName} sudah dibeli.");
            return;
        }

        if (!_currencyManager.SpendMoney(_selectedSkillData.price))
        {
            Debug.Log("[SkillTreeUI] Uang tidak cukup.");
            return;
        }

        // Mark as purchased.
        _selectedSkillData.isPurchased = true;

        Debug.Log($"[SkillTreeUI] Berhasil membeli: {_selectedSkillData.displayName}");

        // Unlock the skill so it can be equipped in the preparation panel.
        _selectedSkillData.locked = false;

        UpdateDetailPanel(_selectedSkillData);
        RefreshUpgradeButton();

        // Refresh the slot that was just purchased.
        if (_selectedSlot != null)
        {
            _selectedSlot.UpdatePurchasedVisual(true);
            _selectedSlot.Refresh();
        }

        // Currency change triggers HandleCurrencyChanged, which refreshes all other slots.
    }

    // ── Cleanup ───────────────────────────────────────────────────────────

    private void ClearList()
    {
        foreach (var obj in _spawnedUIElements)
        {
            if (obj != null) Destroy(obj);
        }

        _spawnedUIElements.Clear();
        _spawnedSlots.Clear();
        _selectedSlot = null;
        _selectedSkillData = null;
    }

    private void CloseUI()
    {
        if (PlayerUI.Instance != null)
            PlayerUI.Instance.CloseAllRolePanels();
        else
            gameObject.SetActive(false);
    }
}