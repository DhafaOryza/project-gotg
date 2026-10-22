using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [Header("Player Profile")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private HealthBar granaryHealthBar;

    [Header("Preparation")]
    [SerializeField] private PreparationUI _preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private Transform _skillContainer;
    [SerializeField] private SkillUI _skillUIPrefab;

    [Header("Consumable & Item Radial Menu")]
    [SerializeField] private GameObject _consumableRadialContainer;
    [SerializeField] private ConsumableRadialUI _consumableRadialUI;

    [Header("Dialog System")]
    [SerializeField] private ConversationUI _conversationUI;

    [Header("Role NPC Panels")]
    [SerializeField] private GranaryUI _granaryUI;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private HealthUI _healthUI;
    [SerializeField] private SkillTreeUI _skillTreeUI;

    [Header("Result")]
    [SerializeField] private ResultUI _resultUI;

    [Header("Transition")]
    [SerializeField] private DayTransitionUI dayTransition;

    private SkillsManager _skillsManager;
    private PlayerController _playerController;
    private PlayerBaseEntity player;
    private GranaryBase _granary;
    private NPCDataSO _npcData;
    private NPCBaseEntity _activeNpc;

    public ConversationUI ConversationUI => _conversationUI;
    public GranaryUI GranaryUI => _granaryUI;
    public ShopUI ShopUI => _shopUI;
    public HealthUI HealthUI => _healthUI;
    public SkillTreeUI SkillTreeUI => _skillTreeUI;
    public ResultUI ResultUI => _resultUI;
    public DayTransitionUI DayTransition => dayTransition;
    public NPCDataSO NpcData => _npcData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeUI();
    }

    void OnEnable()
    {
        if (_granary != null)
            _granary.OnInteract += OpenPreparationUI;
    }

    void OnDisable()
    {
        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;
    }

    /// <summary>
    /// Sambungkan event OnInteract milik granary ke PreparationUI.
    /// Dipanggil oleh GranaryBase saat Start (atau oleh PlayerUI.Start untuk granary yang sudah ada).
    /// </summary>
    public void BindGranary(GranaryBase granary)
    {
        if (_granary == granary) return;

        if (_granary != null)
            _granary.OnInteract -= OpenPreparationUI;

        _granary = granary;

        if (_granary != null && isActiveAndEnabled)
            _granary.OnInteract += OpenPreparationUI;
    }

    /// <summary>
    /// Initializes all core user interface components at game start.
    /// </summary>
    public void InitializeUI()
    {
        if (_preparationUI == null) _preparationUI = Object.FindFirstObjectByType<PreparationUI>();
        if (_conversationUI == null) _conversationUI = Object.FindFirstObjectByType<ConversationUI>();
        if (_granaryUI == null) _granaryUI = Object.FindFirstObjectByType<GranaryUI>();
        if (_healthUI == null) _healthUI = Object.FindFirstObjectByType<HealthUI>();
        if (_shopUI == null) _shopUI = Object.FindFirstObjectByType<ShopUI>();
        if (_skillTreeUI == null) _skillTreeUI = Object.FindFirstObjectByType<SkillTreeUI>();
        if (_resultUI == null) _resultUI = Object.FindFirstObjectByType<ResultUI>();
        if (_resultUI == null) _resultUI = Object.FindFirstObjectByType<ResultUI>(FindObjectsInactive.Include);
        if (dayTransition == null) dayTransition = Object.FindFirstObjectByType<DayTransitionUI>();

        if (_preparationUI != null) _preparationUI.gameObject.SetActive(false);
        if (_skillContainer != null) _skillContainer.gameObject.SetActive(false);
        if (_consumableRadialContainer != null) _consumableRadialContainer.gameObject.SetActive(false);
        
        CloseAllRolePanels();

        if (_granary == null)
        {
            var granary = GameManager.Instance?.levelSpawnerManager?.GetCurrentGranary();
            if (granary != null) BindGranary(granary);
        }

        if (player == null)
        {
            var currentPlayer = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
            player = currentPlayer;
        }

        InitializeHealthBar();
    }

    /// <summary>
    /// Opens the corresponding panel automatically based on the NPC data profile and tracks the active NPC.
    /// </summary>
    public void HandleNpcRolePanel(NPCDataSO npcData, NPCBaseEntity npc = null)
    {
        CloseAllRolePanels();

        if (npcData == null) return;

        _activeNpc = npc; // Simpan referensi NPC

        switch (npcData.NpcRole)
        {
            case NpcRole.Priest:
                if (_healthUI != null) _healthUI.gameObject.SetActive(true);
                break;
            case NpcRole.GranaryKeeper:
                if (_granaryUI != null) _granaryUI.gameObject.SetActive(true);
                break;
            case NpcRole.Merchant:
                if (_shopUI != null) _shopUI.gameObject.SetActive(true);
                break;
            case NpcRole.Guard:
            case NpcRole.GateGuard:
                if (_skillTreeUI != null)
                {
                    _skillTreeUI.Initialize(npcData);
                    _skillTreeUI.gameObject.SetActive(true);
                }
                break;
            default:
                _activeNpc?.ResumeWandering();
                _activeNpc = null;
                break;
        }
    }

    /// <summary>
    /// Closes all role-specific UI panels and resumes active NPC wandering.
    /// </summary>
    public void CloseAllRolePanels()
    {
        CloseHealthUI();
        CloseGranaryUI();
        CloseShopUI();
        CloseSkillTreeUI();

        // Ketika semua panel ditutup, NPC diizinkan bergerak kembali
        if (_activeNpc != null)
        {
            _activeNpc.ResumeWandering();
            _activeNpc = null;
        }
    }

    public void InitializeHealthBar()
    {
        playerHealthBar.Initialize(player);
        granaryHealthBar.Initialize(_granary);
    }

    public void InitializeSkillUI()
    {
        _skillsManager = GameManager.Instance?.skillsManager;
        _playerController = GameManager.Instance?.playerController;

        if (_skillsManager == null || _playerController == null || _skillUIPrefab == null || _skillContainer == null)
            return;

        foreach (Transform child in _skillContainer)
        {
            Destroy(child.gameObject);
        }

        var skills = _skillsManager.choosenSkills;
        var keys = new List<KeyCode>
        {
            _playerController.skill1,
            _playerController.skill2,
            _playerController.skill3,
            _playerController.skill4,
        };

        for (int i = 0; i < skills.Count && i < keys.Count; i++)
        {
            SkillUI currentSkill = Instantiate(_skillUIPrefab, _skillContainer);
            currentSkill.Setup(i, keys[i].ToString(), skills[i]);
        }
    }


    public void OpenPreparationUI() => _preparationUI?.gameObject.SetActive(true);

    public void ClosePreparationUI()
    {
        _preparationUI?.gameObject.SetActive(false);

        if (_granary != null)
            _granary.ResumeSign();
    }

    public void OpenHealthUI() => _healthUI?.gameObject.SetActive(true);
    public void CloseHealthUI() => _healthUI?.gameObject.SetActive(false);

    public void OpenGranaryUI() => _granaryUI?.gameObject.SetActive(true);
    public void CloseGranaryUI() => _granaryUI?.gameObject.SetActive(false);

    public void OpenShopUI() => _shopUI?.gameObject.SetActive(true);
    public void CloseShopUI() => _shopUI?.gameObject.SetActive(false);

    public void OpenSkillTreeUI() => _skillTreeUI?.gameObject.SetActive(true);
    public void CloseSkillTreeUI() => _skillTreeUI?.gameObject.SetActive(false);

    public void OpenSkillUI() => _skillContainer?.gameObject.SetActive(true);
    public void CloseSkillUI() => _skillContainer?.gameObject.SetActive(false);

    public void OpenConsumableUI()
    {
        if (_consumableRadialContainer != null)
            _consumableRadialContainer.gameObject.SetActive(true);
        if (_consumableRadialUI != null)
            _consumableRadialUI.SyncWithManager();
    }
    public void CloseConsumableUI() => _consumableRadialContainer?.gameObject.SetActive(false);
    public void OpenResultUI(NightResult result)
    {
        if (_resultUI == null)
        {
            Debug.LogWarning("[PlayerUI] ResultUI tidak ditemukan di scene.");
            return;
        }

        _resultUI.gameObject.SetActive(true);
        _resultUI.Show(result);
    }
    public void CloseResultUI() => _resultUI?.gameObject.SetActive(false);

    internal void HandleNpcRolePanel(string npcId)
    {
        throw new System.NotImplementedException();
    }
}