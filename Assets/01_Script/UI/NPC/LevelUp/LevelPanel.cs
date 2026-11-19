using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour
{
    private const string MaxLabel = "MAX";

    [Header("Current Level Preview")]
    [SerializeField] private TextMeshProUGUI _txtCurrentLevel;
    [SerializeField] private TextMeshProUGUI _txtCurrentLevelHealth;
    [SerializeField] private SliderBar _currentLevelHealthBar;
    [SerializeField] private TextMeshProUGUI _txtCurrentSkillCooldown;

    [Header("Next Level Preview")]
    [SerializeField] private TextMeshProUGUI _txtNextLevel;
    [SerializeField] private TextMeshProUGUI _txtNextLevelHealth;
    [SerializeField] private SliderBar _nextLevelHealthBar;
    [SerializeField] private TextMeshProUGUI _txtNextSkillCooldown;

    [Header("Level Up")]
    [SerializeField] private TextMeshProUGUI _txtLevelUpCost;
    [SerializeField] private Button _btnLevelUp;


    private PlayerBaseEntity currentPlayer;

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf() => gameObject.SetActive(false);

    void OnEnable()
    {
        Initialize();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    public void Initialize()
    {
        Unsubscribe();

        currentPlayer = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();

        // OnHealthChanged ikut terpanggil saat level up / stat berubah (RecalculateMaxHealth),
        // jadi preview otomatis ter-update tanpa perlu dipanggil manual dari tombol Level Up.
        if (currentPlayer != null)
            currentPlayer.OnHealthChanged += HandleHealthChanged;

        if (_btnLevelUp != null)
            _btnLevelUp.onClick.AddListener(HandleLevelUpButton);

        Refresh();
    }

    /// <summary>Perbarui seluruh preview (level sekarang, level berikutnya, dan harga level up).</summary>
    public void Refresh()
    {
        if (currentPlayer == null) return;

        UpdateCurrentLevel();
        UpdateNextLevel();
        UpdateLevelUpCost();
    }

    #region Helper

    private void Unsubscribe()
    {
        if (currentPlayer != null)
            currentPlayer.OnHealthChanged -= HandleHealthChanged;

        if (_btnLevelUp != null)
            _btnLevelUp.onClick.RemoveListener(HandleLevelUpButton);
    }

    private void HandleHealthChanged(int current, int max)
    {
        Refresh();
    }

    private void UpdateCurrentLevel()
    {
        int level = currentPlayer.CharacterLevel;

        // Teks "Level" sudah ada sebagai label terpisah di bawah angka, jadi di sini cukup angkanya.
        SetText(_txtCurrentLevel, level.ToString());
        SetHealth(_currentLevelHealthBar, _txtCurrentLevelHealth, currentPlayer.CurrentHealth, currentPlayer.MaxHealth);
        SetText(_txtCurrentSkillCooldown, FormatCooldown(currentPlayer.GetCharacterCooldownMultiplierAtLevel(level)));
    }

    private void UpdateNextLevel()
    {
        int level = currentPlayer.CharacterLevel;

        // Sudah level maksimal: tidak ada level berikutnya, tampilkan kondisi sekarang.
        if (!currentPlayer.CanLevelUpCharacter)
        {
            SetText(_txtNextLevel, MaxLabel);
            SetHealth(_nextLevelHealthBar, _txtNextLevelHealth, currentPlayer.CurrentHealth, currentPlayer.MaxHealth);
            SetText(_txtNextSkillCooldown, FormatCooldown(currentPlayer.GetCharacterCooldownMultiplierAtLevel(level)));
            return;
        }

        int nextLevel = level + 1;

        // Meniru BaseEntity.RecalculateMaxHealth: kalau max HP naik, HP sekarang ikut naik
        // sebesar selisihnya (kecuali player sedang mati).
        int maxHealthGain = currentPlayer.GetCharacterMaxHealthAtLevel(nextLevel)
                          - currentPlayer.GetCharacterMaxHealthAtLevel(level);

        int nextMaxHealth = Mathf.Max(1, currentPlayer.MaxHealth + maxHealthGain);
        int nextCurrentHealth = currentPlayer.CurrentHealth;

        if (maxHealthGain > 0 && !currentPlayer.IsDead)
            nextCurrentHealth += maxHealthGain;

        nextCurrentHealth = Mathf.Min(nextCurrentHealth, nextMaxHealth);

        SetText(_txtNextLevel, nextLevel.ToString());
        SetHealth(_nextLevelHealthBar, _txtNextLevelHealth, nextCurrentHealth, nextMaxHealth);
        SetText(_txtNextSkillCooldown, FormatCooldown(currentPlayer.GetCharacterCooldownMultiplierAtLevel(nextLevel)));
    }

    private void UpdateLevelUpCost()
    {
        string cost = currentPlayer.CanLevelUpCharacter
            ? currentPlayer.GetCharacterLevelUpPrice().ToString()
            : MaxLabel;

        SetText(_txtLevelUpCost, cost);
    }

    private void HandleLevelUpButton()
    {
        if (currentPlayer == null) return;

        if (!currentPlayer.CanLevelUpCharacter)
        {
            Debug.LogWarning("LevelPanel: Player is already at max level. Cannot level up.");
            return;
        }

        int price = currentPlayer.GetCharacterLevelUpPrice();

        if (GameManager.Instance.currencyManager.Money < price)
        {
            Debug.LogWarning("LevelPanel: Not enough currency to level up.");
            return;
        }

        // Kurangi uang pemain
        GameManager.Instance.currencyManager.SpendMoney(price);

        // Level up karakter
        currentPlayer.LevelUpCharacter();

        // Perbarui preview
        Refresh();
    }

    private static void SetHealth(SliderBar bar, TextMeshProUGUI label, int current, int max)
    {
        if (bar != null) bar.SetHP(current, max);
        SetText(label, $"{current}/{max}");
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null) target.text = value;
    }

    // GetCharacterCooldownMultiplierAtLevel mengembalikan PENGALI cooldown (mis. 0.98), bukan detik.
    private static string FormatCooldown(float multiplier)
    {
        return $"Cooldown: {multiplier.ToString("0.##", CultureInfo.InvariantCulture)}s";
    }

    #endregion
}