using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Header Result")]
    [SerializeField] private GameObject _clearHeader; // Banner "YOU WIN"
    [SerializeField] private GameObject _loseHeader;  // Banner "YOU LOSE"

    [Header("4 Report Cards (Horizontal Layout Group)")]
    [SerializeField] private ResultCardUI cardSurvivalTime;
    [SerializeField] private ResultCardUI cardEnemiesKilled;
    [SerializeField] private ResultCardUI cardKnockdowns;
    [SerializeField] private ResultCardUI cardGranaryDamage;

    [Header("Card Icons (Sprites)")]
    [SerializeField] private Sprite iconSurvival;
    [SerializeField] private Sprite iconKills;
    [SerializeField] private Sprite iconKnockdown;
    [SerializeField] private Sprite iconGranary;

    [Header("Bottom Reward Bar")]
    [SerializeField] private TextMeshProUGUI _txtRewardAmount;

    [Header("Buttons")]
    [SerializeField] private Button _btnContinue;

    private NightResult _lastResult = NightResult.Clear;

    private void Awake()
    {
        if (_btnContinue != null)
            _btnContinue.onClick.AddListener(HandleContinueButton);
    }

    private void OnDestroy()
    {
        if (_btnContinue != null)
            _btnContinue.onClick.RemoveListener(HandleContinueButton);
    }

    public void Show(NightResult result)
    {
        _lastResult = result;
        bool won = result == NightResult.Clear;

        if (_clearHeader != null) _clearHeader.SetActive(won);
        if (_loseHeader != null) _loseHeader.SetActive(!won);

        ShowReport();
    }

    private void ShowReport()
    {
        bool won = _lastResult == NightResult.Clear;
        GameManager gameManager = GameManager.Instance;
        TrackerBattle tracker = gameManager?.trackerBattle;
        CurrencyManager currency = gameManager?.currencyManager;

        if (tracker == null || currency == null)
        {
            Debug.LogWarning("[ResultUI] TrackerBattle atau CurrencyManager tidak ditemukan.", this);
            return;
        }

        if (!tracker.TryEndBattle(out BattleReport report)) return;

        BattleReward reward = currency.ApplyBattleReward(report, won);
        int minutes = Mathf.FloorToInt(report.SurvivalTime / 60f);
        int seconds = Mathf.FloorToInt(report.SurvivalTime % 60f);
        cardSurvivalTime?.Setup(iconSurvival, "Survival Time", $"{minutes:00}:{seconds:00}");

        cardEnemiesKilled?.Setup(iconKills, "Enemies Killed", $"{report.EnemiesKilled}/{report.EnemiesSpawned}");

        cardKnockdowns?.Setup(iconKnockdown, "Knockdowns", $"{report.PlayerKnockdowns}");

        cardGranaryDamage?.Setup(iconGranary, "Granary Damage", $"{report.GranaryDamage}");

        if (_txtRewardAmount != null)
        {
            _txtRewardAmount.text = $"+{reward.Money} Coins";
        }
    }

    public void HandleContinueButton()
    {
        var gameManager = GameManager.Instance;
        if (gameManager == null) return;

        GameState target = _lastResult == NightResult.Clear ? GameState.Reset : GameState.Starting;

        if (gameManager.SetState(target))
        {
            gameObject.SetActive(false);
        }
    }
}