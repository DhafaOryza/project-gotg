using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button btnPreparation;
    [SerializeField] private Button btnExploreWorld;
    [SerializeField] private Button btnStartNight;
    [SerializeField] private TextMeshProUGUI txtNightCounter;

    [Header("Panel")]
    [SerializeField] private GameObject preparationPanel;
    [SerializeField] private Button btnPrepareBack;

    [Header("Prepare Tabs")]
    [SerializeField] private Button btnSkillTab;
    [SerializeField] private Button btnItemsTab;
    [SerializeField] private GameObject skillTabPanel;
    [SerializeField] private GameObject itemsTabPanel;

    [Header("Prepare Skill UI")]
    [SerializeField] private SkillCard skillCardPrefab;
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillSlotUI slotUIPrefab;
    [SerializeField] private Transform skillSlotContainer;

    [Header("Prepare Items UI")]
    [SerializeField] private GameObject itemsCardPrefab;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;
    [SerializeField] private Transform itemSlotContainer;

    [Header("Notification")]
    [SerializeField] private Notification notification;

    private NightController nightController;

    private bool hasWarned = false;

    // Listener tombol didaftarkan SEKALI di Awake. Sebelumnya didaftarkan di Initialize() yang
    // dipanggil setiap OnEnable, sehingga tiap panel dibuka listener-nya bertambah (aksi terpanggil berkali-kali).
    void Awake()
    {
        if (btnPreparation != null) btnPreparation.onClick.AddListener(HandlePreparationButton);
        if (btnExploreWorld != null) btnExploreWorld.onClick.AddListener(HandleExploreWorldButton);
        if (btnStartNight != null) btnStartNight.onClick.AddListener(HandleStartNightButton);
        if (btnPrepareBack != null) btnPrepareBack.onClick.AddListener(HandleBackPreparationButton);
        if (btnSkillTab != null) btnSkillTab.onClick.AddListener(ShowSkillTab);
        if (btnItemsTab != null) btnItemsTab.onClick.AddListener(ShowItemsTab);
    }

    void OnDestroy()
    {
        if (btnPreparation != null) btnPreparation.onClick.RemoveListener(HandlePreparationButton);
        if (btnExploreWorld != null) btnExploreWorld.onClick.RemoveListener(HandleExploreWorldButton);
        if (btnStartNight != null) btnStartNight.onClick.RemoveListener(HandleStartNightButton);
        if (btnPrepareBack != null) btnPrepareBack.onClick.RemoveListener(HandleBackPreparationButton);
        if (btnSkillTab != null) btnSkillTab.onClick.RemoveListener(ShowSkillTab);
        if (btnItemsTab != null) btnItemsTab.onClick.RemoveListener(ShowItemsTab);
    }

    void OnEnable()
    {
        Initialize();

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
            skillsManager.OnChosenSkillsChanged += RedrawPrepareSkill;

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager != null)
            itemManager.OnChosenItemsChanged += RedrawPrepareItems;

        hasWarned = false;
    }

    void OnDisable()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager != null)
            skillsManager.OnChosenSkillsChanged -= RedrawPrepareSkill;

        var itemManager = GameManager.Instance?.itemManager;
        if (itemManager != null)
            itemManager.OnChosenItemsChanged -= RedrawPrepareItems;
    }

    void Initialize()
    {
        // Night Counter
        HandleNightCounter();

        // Prepare Skill Panel selalu mulai tertutup
        if (preparationPanel != null) preparationPanel.SetActive(false);
        ShowSkillTab();
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
        if (preparationPanel == null) return;

        if (PlayerUI.Instance != null)
            PlayerUI.Instance.ShowPreparationSkillPanel(preparationPanel);
        else
            preparationPanel.SetActive(true);

        GenerateSkillCard();
        GenerateSkillSlot();
        RedrawPrepareItems();
        ShowSkillTab();
    }

    public void ShowSkillTab()
    {
        if (skillTabPanel != null) skillTabPanel.SetActive(true);
        if (itemsTabPanel != null) itemsTabPanel.SetActive(false);
    }

    public void ShowItemsTab()
    {
        if (skillTabPanel != null) skillTabPanel.SetActive(false);
        if (itemsTabPanel != null) itemsTabPanel.SetActive(true);
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

        var skillsManager = GameManager.Instance?.skillsManager;

        if (skillsManager != null && skillsManager.EquippedCount == 0)
        {
            if (!hasWarned)
            {
                if (notification != null)
                {
                    notification.ShowWarning();
                }
                hasWarned = true;
                return;
            }
        }

        controller.StartNight();
    }

    public bool TryClosePrepareSkillPanel()
    {
        if (preparationPanel == null || !preparationPanel.activeSelf)
            return false;

        ClosePrepareSkillPanel();
        return true;
    }

    private void ClosePrepareSkillPanel()
    {
        if (preparationPanel != null)
            preparationPanel.SetActive(false);

        PlayerUI.Instance?.FocusPreparationPanel();
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
            SkillSlotUI slot = Instantiate(slotUIPrefab, skillSlotContainer);

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

    public void RedrawPrepareItems()
    {
        GenerateItemCards();
        GenerateItemSlots();
    }

    private void GenerateItemCards()
    {
        ClearContainer(itemsContainer);

        if (itemsContainer == null || itemsCardPrefab == null)
            return;

        var manager = GameManager.Instance?.itemManager;
        if (manager == null)
            return;

        foreach (var owned in manager.Inventory)
        {
            if (owned == null || owned.data == null) continue;

            // An equipped item is shown in its slot, so it is not listed as a card.
            if (manager.IsEquipped(owned.data)) continue;

            var cardObject = Instantiate(itemsCardPrefab, itemsContainer);
            var card = cardObject.GetComponent<ItemCardUI>();
            if (card == null)
            {
                Debug.LogError("PreparationUI: Items Card Prefab memerlukan komponen ItemCardUI.", cardObject);
                Destroy(cardObject);
                continue;
            }

            card.Initialize(owned.data);
            card.SetDraggable(true);

            card.OnSelected += HandleItemCardSelected;
        }
    }

    /// <summary>
    /// Handles item card selection to automatically equip it into the first available loadout slot.
    /// </summary>
    private void HandleItemCardSelected(ItemCardUI selectedCard)
    {
        var manager = GameManager.Instance?.itemManager;
        if (manager == null || selectedCard == null || selectedCard.Data == null) return;

        // Cari slot kosong pertama dari loadout item (0 hingga MaxSlots - 1)
        for (int i = 0; i < ItemManager.MaxSlots; i++)
        {
            if (manager.GetItem(i) == null)
            {
                manager.EquipItems(i, selectedCard.Data);
                break;
            }
        }
    }

    private void GenerateItemSlots()
    {
        ClearContainer(itemSlotContainer);
        if (itemSlotContainer == null || itemSlotPrefab == null) return;

        var manager = GameManager.Instance?.itemManager;
        var playerController = GameManager.Instance?.playerController;

        for (int slotIndex = 0; slotIndex < ItemManager.MaxSlots; slotIndex++)
        {
            var slot = Instantiate(itemSlotPrefab, itemSlotContainer);
            var runtime = manager != null ? manager.GetRuntimeSlot(slotIndex) : null;
            var data = runtime != null && runtime.HasStock ? runtime.data : null;
            string slotLabel = playerController != null
                ? playerController.GetItemDisplayName(slotIndex)
                : $"Item {slotIndex + 1}";

            slot.SetupLoadoutSlot(slotIndex, slotLabel, data, runtime != null ? runtime.currentStack : 0);
        }
    }

    private static void ClearContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    #endregion
}
