using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public enum StatsType
{
    Vitality,   // Additional for base health
    Endurance,  // Additional for movement speed
    Lucky,      // Additional for critical chance
    Defense,    // Additional for defense decrease enemy damage by percetage
    Haste,      // Additional for reduce cooldown
}

[CreateAssetMenu(fileName = "NewStatsData", menuName = "Data/Stats/Stats Data")]
public class AdditionalStatsSO : ScriptableObject
{
    // Harga dibulatkan ke kelipatan ini supaya angkanya rapi (mis. 562.5 -> 565).
    private const int PriceRounding = 5;

    [Header("References")]
    [SerializeField] private Sprite _statsIcon;
    [SerializeField, TextArea()] private string _description;

    [Header("Stat")]
    [SerializeField] private StatsType statsType;

    [Tooltip("Level saat ini. Otomatis dibatasi antara 1 dan Max Level.")]
    [Min(1)]
    [SerializeField] private int level = 1;

    [Tooltip("Level tertinggi yang bisa dicapai stat ini.")]
    [Min(1)]
    [SerializeField] private int maxLevel = 5;

    [Tooltip("Poin per level. Poin total = baseValue x level.")]
    [SerializeField] private int baseValue;

    [Header("Conversion")]
    [Tooltip("Nilai hasil kalkulasi untuk tiap 1 poin.\n" +
             "Vitality: poin vitality (1 poin = 1 vitality, lalu dikali hpPerVitality).\n" +
             "Endurance: tambahan moveSpeed per poin (mis. 0.1).\n" +
             "Defense: persen pengurangan damage per poin (mis. 1 = 1%).\n" +
             "Haste: persen pengurangan cooldown per poin (mis. 1 = 1%).")]
    [SerializeField] private float valuePerPoint = 1f;

    [Tooltip("Batas atas TOTAL hasil kalkulasi untuk jenis stat ini (semua asset sejenis dijumlahkan, lalu dibatasi). " +
             "0 = tanpa batas. Contoh: Defense 75, Haste 50.")]
    [SerializeField] private float maxTotalValue = 0f;

    [Header("Upgrade Price")]
    [Tooltip("Harga upgrade dari level 1 ke level 2.")]
    [Min(0)]
    [SerializeField] private int basePrice = 250;

    [Tooltip("Pengali harga tiap naik level.\n" +
             "1 = harga tetap di semua level.\n" +
             "1.5 = tiap level 50% lebih mahal dari level sebelumnya.")]
    [Min(1f)]
    [SerializeField] private float priceGrowth = 1.5f;

    #region Properties

    public Sprite Icon => _statsIcon;
    public string Description => _description;

    public StatsType Type => statsType;
    public int Level => ClampedLevel;
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public int Value => baseValue;
    public float ValuePerPoint => valuePerPoint;
    public float MaxTotalValue => maxTotalValue;
    public int BasePrice => basePrice;

    public bool IsMaxLevel => ClampedLevel >= MaxLevel;

    // Level selalu dijaga di rentang 1..maxLevel, bahkan kalau field level diisi asal di Inspector.
    private int ClampedLevel => Mathf.Clamp(level, 1, Mathf.Max(1, maxLevel));

    #endregion

    #region Level

    /// <summary>
    /// Naikkan level satu tingkat. Return false kalau sudah di level maksimal.
    /// Setelah ini panggil PlayerBaseEntity.RecalculateMaxHealth() (atau gunakan
    /// PlayerBaseEntity.LevelUpAdditionalStat yang sudah melakukannya).
    /// </summary>
    public bool TryLevelUp()
    {
        if (IsMaxLevel) return false;

        level = ClampedLevel + 1;
        return true;
    }

    public void ResetLevel()
    {
        level = 1;
    }

    private void OnValidate()
    {
        maxLevel = Mathf.Max(1, maxLevel);
        level = Mathf.Clamp(level, 1, maxLevel);
    }

    #endregion

    #region Calculation

    /// <summary>Jumlah poin stat ini: baseValue dikali level.</summary>
    public float GetPoints()
    {
        return baseValue * ClampedLevel;
    }

    /// <summary>
    /// Nilai hasil kalkulasi satu asset di level saat ini: poin dikali valuePerPoint.
    /// Artinya bergantung jenisnya: poin vitality, tambahan moveSpeed, persen pengurangan damage,
    /// atau persen pengurangan cooldown.
    /// </summary>
    public float GetCalculatedValue()
    {
        return GetPoints() * valuePerPoint;
    }

    /// <summary>Nilai hasil kalkulasi kalau stat ini berada di level tertentu (untuk pratinjau upgrade).</summary>
    public float GetCalculatedValueAtLevel(int atLevel)
    {
        return baseValue * Mathf.Clamp(atLevel, 1, MaxLevel) * valuePerPoint;
    }

    /// <summary>
    /// Total hasil kalkulasi semua stat sejenis dalam satu daftar, lalu dibatasi maxTotalValue.
    /// Kalau beberapa asset sejenis punya batas berbeda, dipakai batas terkecil yang bukan 0.
    /// </summary>
    public static float CalculateTotal(StatsType type, IEnumerable<AdditionalStatsSO> stats)
    {
        if (stats == null) return 0f;

        float total = 0f;
        float cap = 0f;

        foreach (var stat in stats)
        {
            if (stat == null || stat.statsType != type) continue;

            total += stat.GetCalculatedValue();

            if (stat.maxTotalValue > 0f && (cap <= 0f || stat.maxTotalValue < cap))
                cap = stat.maxTotalValue;
        }

        return cap > 0f ? Mathf.Min(total, cap) : total;
    }

    #endregion

    #region Upgrade Price

    /// <summary>
    /// Harga untuk naik dari level saat ini ke level berikutnya. 0 kalau sudah level maksimal.
    /// </summary>
    public int GetUpgradePrice()
    {
        return IsMaxLevel ? 0 : GetUpgradePriceFromLevel(ClampedLevel);
    }

    /// <summary>
    /// Harga untuk naik dari level fromLevel ke fromLevel + 1:
    ///   harga = basePrice x priceGrowth ^ (fromLevel - 1), dibulatkan ke kelipatan 5.
    /// Contoh (basePrice 250, priceGrowth 1.5): Lv1->2 = 250, Lv2->3 = 375, Lv3->4 = 565, Lv4->5 = 845.
    /// </summary>
    public int GetUpgradePriceFromLevel(int fromLevel)
    {
        float raw = basePrice * Mathf.Pow(priceGrowth, Mathf.Max(0, fromLevel - 1));
        return Mathf.FloorToInt(raw / PriceRounding + 0.5f) * PriceRounding;
    }

    /// <summary>Total harga dari level saat ini sampai level maksimal (berguna untuk menyeimbangkan ekonomi).</summary>
    public int GetTotalPriceToMax()
    {
        int total = 0;
        for (int l = ClampedLevel; l < MaxLevel; l++)
            total += GetUpgradePriceFromLevel(l);

        return total;
    }

    #endregion
}