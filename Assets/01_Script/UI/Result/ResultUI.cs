using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    /// <summary>
    /// The texts of one panel. The clear panel and the lose panel each have their own set.
    /// </summary>
    [Serializable]
    private class ReportTexts
    {
        [SerializeField] private TextMeshProUGUI _survivalTimeText;
        [SerializeField] private TextMeshProUGUI _killText;
        [SerializeField] private TextMeshProUGUI _knockdownText;
        [SerializeField] private TextMeshProUGUI _granaryDamageText;
        [SerializeField] private TextMeshProUGUI _killPercentText;
        [SerializeField] private TextMeshProUGUI _totalPercentText;
        [SerializeField] private TextMeshProUGUI _rewardText;

        public void Apply(BattleReport report, BattleReward reward)
        {
            int minutes = Mathf.FloorToInt(report.SurvivalTime / 60f);
            int seconds = Mathf.FloorToInt(report.SurvivalTime % 60f);

            if (_survivalTimeText != null) _survivalTimeText.text = $"{minutes:00}:{seconds:00}";
            if (_killText != null) _killText.text = $"{report.EnemiesKilled}/{report.EnemiesSpawned}";
            if (_knockdownText != null) _knockdownText.text = $"{report.PlayerKnockdowns}";
            if (_granaryDamageText != null) _granaryDamageText.text = $"{report.GranaryDamage}";
            if (_killPercentText != null) _killPercentText.text = FormatPercent(reward.KillPercent);
            if (_totalPercentText != null) _totalPercentText.text = FormatPercent(reward.FinalPercent);
            if (_rewardText != null) _rewardText.text = $"+{reward.Money}";
        }

        private static string FormatPercent(float value)
        {
            return $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }

    [Header("Panels")]
    [FormerlySerializedAs("clear")]
    [SerializeField] private GameObject _clear;
    [FormerlySerializedAs("lose")]
    [SerializeField] private GameObject _lose;

    [Header("Shared Report")]
    [SerializeField] private GameObject _reportPanel;
    [SerializeField] private ReportTexts _reportTexts;

    [Header("References")]
    [FormerlySerializedAs("btnContinue")]
    [SerializeField] private Button _btnContinue;
    [SerializeField] private GameObject _wavemeter;

    private NightResult _lastResult = NightResult.Clear;

    // Listener didaftarkan SEKALI di Awake. Sebelumnya Initialize() dipanggil di Awake dan OnEnable,
    // sehingga tiap ResultUI dibuka listener-nya bertambah (ProceedToNextNight terpanggil berkali-kali).
    void Awake()
    {
        if (_btnContinue != null)
            _btnContinue.onClick.AddListener(HandleContinueButton);
    }

    void OnDestroy()
    {
        if (_btnContinue != null)
            _btnContinue.onClick.RemoveListener(HandleContinueButton);
    }

    /// <summary>Tampilkan panel sesuai hasil night, lengkap dengan laporan battle.</summary>
    public void Show(NightResult result)
    {
        _lastResult = result;
        bool won = result == NightResult.Clear;

        if (_clear != null) _clear.SetActive(won);
        if (_lose != null) _lose.SetActive(!won);
        if (_reportPanel != null) _reportPanel.SetActive(true);

        ShowReport();
    }

    public void ClearNightWave() => Show(NightResult.Clear);
    public void LoseNightWave() => Show(NightResult.Lose);

    private void ShowReport()
    {
        bool won = _lastResult == NightResult.Clear;
        GameManager gameManager = GameManager.Instance;
        TrackerBattle tracker = gameManager != null ? gameManager.trackerBattle : null;
        CurrencyManager currency = gameManager != null ? gameManager.currencyManager : null;

        if (tracker == null || currency == null)
        {
            Debug.LogWarning("[ResultUI] TrackerBattle atau CurrencyManager tidak ditemukan.", this);
            return;
        }

        // A battle can only end once, so the reward is never paid twice.
        if (!tracker.TryEndBattle(out BattleReport report)) return;

        BattleReward reward = currency.ApplyBattleReward(report, won);

        _reportTexts?.Apply(report, reward);
    }

    // Semua alur setelah Continue diatur GameManager lewat GameState:
    //   menang -> Reset    (night berikutnya, NPC kembali, interactable aktif)
    //   kalah  -> Starting (dunia di-reset: HP penuh, NPC kembali)
    public void HandleContinueButton()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("[ResultUI] GameManager tidak ditemukan.", this);
            return;
        }

        GameState target = _lastResult == NightResult.Clear ? GameState.Reset : GameState.Starting;

        // Panel hanya ditutup kalau perpindahan state diterima (mis. ditolak saat transisi masih berjalan).
        if (gameManager.SetState(target))
        {
            gameObject.SetActive(false);
        }
            
        var meter = _wavemeter.GetComponent<WaveMeterUI>();
        if (meter != null)
            meter.ResetWaveMeter();

        if (_wavemeter != null)
            _wavemeter.gameObject.SetActive(false);
        
    }
}
