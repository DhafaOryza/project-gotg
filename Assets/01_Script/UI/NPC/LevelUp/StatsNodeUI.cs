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
    }

    void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelectButton);

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

    #endregion

    #region Upgrade

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
            return;
        }

        if (_statsIcon != null) _statsIcon.sprite = stats.Icon;
        if (_statsType != null) _statsType.text = stats.Type.ToString();
        if (_statsDesc != null) _statsDesc.text = stats.Description;
        if (_statsLevel != null) _statsLevel.text = $"Lv. {stats.Level}/{stats.MaxLevel}";
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
}