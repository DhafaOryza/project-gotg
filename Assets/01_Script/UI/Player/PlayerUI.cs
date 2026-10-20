using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; set; }

    [SerializeField] private PreparationUI preparationUI;

    [Header("Skill Panel")]
    [SerializeField] private SkillUI skillUIPrefab;
    [SerializeField] private Transform skillContainer;

    [Header("Health Bar Player")]
    [SerializeField] private HealthBar playerHealthBar;

    [Header("Health Bar Granary")]
    [SerializeField] private HealthBar granaryHealthBar;

    private SkillsManager skillsManager;
    private PlayerController playerController;

    private BaseEntity playerEntity;
    private BuildingBase buildingBase;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            return;
        }
    }

    void Start()
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        if (preparationUI == null) preparationUI = Object.FindFirstObjectByType<PreparationUI>();

        var spawner = GameManager.Instance?.levelSpawnerManager;

        playerEntity = spawner.GetCurrentPlayer() as BaseEntity;
        buildingBase = spawner.GetCurrentGranary() as BuildingBase;

        if (preparationUI != null) preparationUI.gameObject.SetActive(false);
        if (skillContainer != null) skillContainer.gameObject.SetActive(false);

        InitializeHealthBar();
    }

    #region Health Bar

    public void InitializeHealthBar()
    {
        playerHealthBar.Initialize(playerEntity);
        granaryHealthBar.Initialize(buildingBase);
    }

    #endregion

    #region SkillUI

    public void InitializeSkillUI()
    {
        skillsManager = GameManager.Instance?.skillsManager;
        playerController = GameManager.Instance?.playerController;

        if (skillsManager == null || playerController == null || skillUIPrefab == null || skillContainer == null)
            return;

        // Bersihkan dulu: tanpa ini, setiap pemanggilan menambah slot baru di atas yang lama,
        // dan child bawaan di container (placeholder) ikut tertinggal.
        ClearSkillUI();

        var keys = new List<KeyCode>
        {
            playerController.skill1,
            playerController.skill2,
            playerController.skill3,
            playerController.skill4,
        };

        // Satu UI per slot (bukan per skill terpasang): indeks i selalu = tombol keys[i],
        // sama dengan yang dipakai PlayerController dan PlayerBaseEntity.
        // Slot tanpa tombol (maxSkills > jumlah tombol) tidak ditampilkan karena tidak bisa dipakai.
        int slotCount = Mathf.Min(skillsManager.maxSkills, keys.Count);

        for (int i = 0; i < slotCount; i++)
        {
            SkillDataSO skill = skillsManager.GetSkill(i); // null kalau slot kosong
            SkillUI currentSkill = Instantiate(skillUIPrefab, skillContainer);
            currentSkill.Setup(keys[i].ToString(), skill);
        }
    }

    private void ClearSkillUI()
    {
        for (int i = skillContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(skillContainer.GetChild(i).gameObject);
        }
    }

    #endregion

    public void OpenPreparationUI()
    {
        preparationUI.gameObject.SetActive(true);
    }

    public void ClosePreparationUI()
    {
        preparationUI.gameObject.SetActive(false);
    }

    public void OpenSkillUI()
    {
        skillContainer.gameObject.SetActive(true);
    }

    public void CloseSkillUI()
    {
        skillContainer.gameObject.SetActive(false);
    }
}