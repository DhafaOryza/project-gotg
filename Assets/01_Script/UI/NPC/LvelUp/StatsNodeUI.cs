using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsNodeUI : MonoBehaviour
{
    [SerializeField] private Image _statsIcon;
    [SerializeField] private TextMeshProUGUI _statsType;
    [SerializeField] private TextMeshProUGUI _statsDesc;
    [SerializeField] private TextMeshProUGUI _statsLevel;
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TextMeshProUGUI _upgradePrice;

    [Header("Price Display")]
    [Tooltip("Teks harga saat stat sudah level maksimal.")]
    [SerializeField] private string _maxLevelText = "MAX";
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(1f, 0.4f, 0.4f);

    private AdditionalStatsSO stats;
    private CurrencyManager currencyManager;

    /// <summary>Dipicu setelah upgrade berhasil, supaya panel induk bisa menyegarkan node lain.</summary>
    public event Action<StatsNodeUI> OnUpgraded;
    public event Action<StatsNodeUI> OnSelected;

    public AdditionalStatsSO Stats => stats;

    // Listener tombol didaftarkan sekali di Awake (jangan di Initialize, supaya tidak menumpuk).
    void Awake()
    {
        if (_selectButton == null)
            _selectButton = GetComponent<Button>();

        if (_selectButton != null)
            _selectButton.onClick.AddListener(HandleSelectButton);

        if (_upgradeButton != null)
            _upgradeButton.onClick.AddListener(HandleUpgradeButton);
    }

    void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelectButton);

        if (_upgradeButton != null)
            _upgradeButton.onClick.RemoveListener(HandleUpgradeButton);

        UnsubscribeCurrency();
    }

    public void Initialize(AdditionalStatsSO statsSO)
    {
        stats = statsSO;

        // CurrencyManager baru diambil di sini (bukan di Awake): saat Awake node baru dibuat,
        // currencyManager belum ada. Langganan sebelumnya dilepas dulu supaya tidak ganda.
        UnsubscribeCurrency();
        currencyManager = GameManager.Instance?.currencyManager;
        if (currencyManager != null)
            currencyManager.OnChange += HandleCurrencyChanged;

        Refresh();
    }

    #region Validation

    /// <summary>True kalau uang cukup untuk membayar sejumlah harga.</summary>
    public bool CanAfford(int price)
    {
        return currencyManager != null && price >= 0 && currencyManager.Money >= price;
    }

    /// <summary>True kalau stat belum level maksimal DAN uang cukup untuk upgrade berikutnya.</summary>
    public bool CanUpgrade()
    {
        if (stats == null || stats.IsMaxLevel) return false;

        return CanAfford(stats.GetUpgradePrice());
    }

    #endregion

    #region Upgrade

    private void HandleUpgradeButton()
    {
        OnSelected?.Invoke(this);

        // Validasi ulang saat diklik: uang bisa berubah sejak tampilan terakhir disegarkan.
        if (!CanUpgrade())
        {
            Refresh();
            return;
        }

        var player = GetPlayer();
        if (player == null) return;

        int price = stats.GetUpgradePrice();

        if (!currencyManager.SpendMoney(price))
        {
            Refresh();
            return;
        }

        // Level naik + max HP dihitung ulang (Vitality). Kalau gagal (mis. sudah maksimal), uang dikembalikan.
        if (!player.LevelUpAdditionalStat(stats))
        {
            currencyManager.AddMoney(price);
            Refresh();
            return;
        }

        Refresh();
        OnUpgraded?.Invoke(this);
    }

    private void HandleSelectButton()
    {
        OnSelected?.Invoke(this);
    }

    #endregion

    #region Display

    /// <summary>Perbarui semua teks dan status tombol. Aman dipanggil kapan saja.</summary>
    public void Refresh()
    {
        if (stats == null)
        {
            if (_statsIcon != null) _statsIcon.sprite = null;
            if (_statsType != null) _statsType.text = string.Empty;
            if (_statsDesc != null) _statsDesc.text = string.Empty;
            if (_statsLevel != null) _statsLevel.text = string.Empty;
            if (_upgradePrice != null) _upgradePrice.text = string.Empty;
            if (_upgradeButton != null) _upgradeButton.interactable = false;
            return;
        }

        if (_statsIcon != null) _statsIcon.sprite = stats.Icon;
        if (_statsType != null) _statsType.text = stats.Type.ToString();
        if (_statsDesc != null) _statsDesc.text = stats.Description;
        if (_statsLevel != null) _statsLevel.text = $"Lv. {stats.Level}/{stats.MaxLevel}";

        bool canUpgrade = CanUpgrade();

        if (_upgradePrice != null)
        {
            _upgradePrice.text = stats.IsMaxLevel ? _maxLevelText : stats.GetUpgradePrice().ToString();

            // Harga berwarna merah kalau uang tidak cukup (khusus saat belum level maksimal).
            _upgradePrice.color = stats.IsMaxLevel || canUpgrade ? _affordableColor : _unaffordableColor;
        }

        if (_upgradeButton != null)
            _upgradeButton.interactable = canUpgrade;
    }

    // Uang berubah (dari mana pun: hadiah, belanja, upgrade lain): tombol dan warna harga ikut diperbarui.
    private void HandleCurrencyChanged()
    {
        Refresh();
    }

    #endregion

    private void UnsubscribeCurrency()
    {
        if (currencyManager != null)
            currencyManager.OnChange -= HandleCurrencyChanged;
    }

    private PlayerBaseEntity GetPlayer()
    {
        return GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
    }
}