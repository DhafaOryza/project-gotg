using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Controller Navigation")]
    [Tooltip("EventSystem scene. Boleh dikosongkan; akan dicari otomatis saat game berjalan.")]
    [SerializeField] private EventSystem eventSystem;
    [Tooltip("Elemen pertama yang dipilih saat kembali ke Preparation UI.")]
    [SerializeField] private GameObject preparationFirstSelected;
    [Tooltip("Elemen pertama yang dipilih saat Prepare Skill Panel dibuka.")]
    [SerializeField] private GameObject prepareSkillFirstSelected;

    private NightController nightController;

    // Listener tombol didaftarkan SEKALI di Awake. Sebelumnya didaftarkan di Initialize() yang
    // dipanggil setiap OnEnable, sehingga tiap panel dibuka listener-nya bertambah (aksi terpanggil berkali-kali).
    void Awake()
    {
        if (eventSystem == null)
            eventSystem = EventSystem.current != null
                ? EventSystem.current
                : FindFirstObjectByType<EventSystem>();

        if (btnPreparation != null) btnPreparation.onClick.AddListener(HandlePreparationButton);
        if (btnExploreWorld != null) btnExploreWorld.onClick.AddListener(HandleExploreWorldButton);
        if (btnStartNight != null) btnStartNight.onClick.AddListener(HandleStartNightButton);
        if (btnPrepareBack != null) btnPrepareBack.onClick.AddListener(HandleBackPreparationButton);
    }

    void OnDestroy()
    {
        if (btnPreparation != null) btnPreparation.onClick.RemoveListener(HandlePreparationButton);
        if (btnExploreWorld != null) btnExploreWorld.onClick.RemoveListener(HandleExploreWorldButton);
        if (btnStartNight != null) btnStartNight.onClick.RemoveListener(HandleStartNightButton);
        if (btnPrepareBack != null) btnPrepareBack.onClick.RemoveListener(HandleBackPreparationButton);
    }

    void OnEnable()
    {
        Initialize();

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
            skillsManager.OnChosenSkillsChanged += RedrawPrepareSkill;
    }

    void OnDisable()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
            skillsManager.OnChosenSkillsChanged -= RedrawPrepareSkill;
    }

    void Initialize()
    {
        // Night Counter
        HandleNightCounter();

        // Prepare Skill Panel selalu mulai tertutup
        if (preparationSkillPanel != null) preparationSkillPanel.SetActive(false);
    }

    private NightController GetNightController()
    {
        if (nightController == null)
            nightController = GameManager.Instance?.nightController;

        return nightController;
    }

    private void HandleNightCounter()
    {
        var controller = GetNightController();
        if (txtNightCounter == null || controller == null) return;

        txtNightCounter.text = $"Night {controller.CurrentNightIndex + 1}";
    }

    private void HandlePreparationButton()
    {
        if (preparationSkillPanel == null) return;

        preparationSkillPanel.SetActive(true);
        GenerateSkillCard();
        GenerateSkillSlot();
        SelectFirst(prepareSkillFirstSelected != null
            ? prepareSkillFirstSelected
            : btnPrepareBack != null ? btnPrepareBack.gameObject : null);
    }

    private void HandleBackPreparationButton()
    {
        ClosePrepareSkillPanel();
    }

    // Tutup panel Preparation lewat PlayerUI, supaya sign interact granary yang tadi
    // disembunyikan ikut muncul lagi. Fallback: tutup langsung kalau PlayerUI tidak ada.
    private void HandleExploreWorldButton()
    {
        if (PlayerUI.Instance != null)
            PlayerUI.Instance.ClosePreparationUI();
        else
            gameObject.SetActive(false);
    }

    private void HandleStartNightButton()
    {
        var controller = GetNightController();
        if (controller == null)
        {
            Debug.LogError("PreparationUI: NightController tidak ditemukan di GameManager.", this);
            return;
        }

        controller.StartNight();
    }

    public bool TryClosePrepareSkillPanel()
    {
        if (preparationSkillPanel == null || !preparationSkillPanel.activeSelf)
            return false;

        ClosePrepareSkillPanel();
        return true;
    }

    private void ClosePrepareSkillPanel()
    {
        if (preparationSkillPanel != null)
            preparationSkillPanel.SetActive(false);

        SelectFirst(preparationFirstSelected != null
            ? preparationFirstSelected
            : btnPreparation != null ? btnPreparation.gameObject : null);
    }

    private void SelectFirst(GameObject target)
    {
        if (target == null || !target.activeInHierarchy) return;

        if (eventSystem == null)
            eventSystem = EventSystem.current != null
                ? EventSystem.current
                : FindFirstObjectByType<EventSystem>();

        if (eventSystem == null) return;

        // firstSelectedGameObject menyimpan default EventSystem, sementara SetSelectedGameObject
        // langsung memindahkan fokus navigasi pada frame ini.
        eventSystem.firstSelectedGameObject = target;
        eventSystem.SetSelectedGameObject(target);
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

        var keys = new List<string>
        {
            playerController.GetSkillDisplayName(0),
            playerController.GetSkillDisplayName(1),
            playerController.GetSkillDisplayName(2),
            playerController.GetSkillDisplayName(3),
        };

        ClearSkillSlot();

        for (int i = 0; i < skillsManager.maxSkills; i++)
        {
            SlotUI slot = Instantiate(slotUIPrefab, skillSlotContainer);

            string key = (i == SkillsManager.PassiveSlotIndex) ? "" : (i < keys.Count ? keys[i] : "");
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
