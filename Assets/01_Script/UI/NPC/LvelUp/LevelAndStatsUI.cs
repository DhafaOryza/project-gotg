using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelAndStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _levelUpTabButton;
    [SerializeField] private Button _statsTabButton;
    [SerializeField] private GameObject _levelUpPanel;
    [SerializeField] private GameObject _statsPanel;
    [SerializeField] private LevelUpDetail _levelUpDetail;
    [SerializeField] private Transform _statsNodeContainer;
    [SerializeField] private StatsNodeUI _statsNodePrefab;
    [SerializeField] private StatsDetailUI _statsDetailUI;

    [Header("Currency References")]
    [SerializeField] private TextMeshProUGUI _txtCurrency;

    private PlayerBaseEntity playerBaseEntity;
    private CurrencyManager currencyManager;
    private readonly List<StatsNodeUI> nodes = new List<StatsNodeUI>();
    private StatsNodeUI selectedNode;

    void Awake()
    {
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(CloseUI);
        }

        if (_levelUpTabButton != null)
            _levelUpTabButton.onClick.AddListener(ShowLevelUpPanel);

        if (_statsTabButton != null)
            _statsTabButton.onClick.AddListener(ShowStatsPanel);
    }

    void OnDestroy()
    {
        if (_backButton != null)
            _backButton.onClick.RemoveListener(CloseUI);

        if (_levelUpTabButton != null)
            _levelUpTabButton.onClick.RemoveListener(ShowLevelUpPanel);

        if (_statsTabButton != null)
            _statsTabButton.onClick.RemoveListener(ShowStatsPanel);
    }

    void OnEnable()
    {
        // currencyManager harus diambil SEBELUM berlangganan (sebelumnya masih null saat panel pertama kali dibuka).
        currencyManager = GameManager.Instance?.currencyManager;
        if (currencyManager != null)
            currencyManager.OnChange += OnChangeCurrency;

        Initialize();
    }

    void OnDisable()
    {
        if (currencyManager != null)
            currencyManager.OnChange -= OnChangeCurrency;

        ClearNodes();
    }

    public void Initialize()
    {
        // Player dicari saat panel dibuka (bukan di Awake): player bisa baru di-spawn setelah panel ini dibuat.
        playerBaseEntity = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();

        InitializeStatsNode();
        IntializeCurrency();
        ShowLevelUpPanel();
    }

    private void CloseUI()
    {
        gameObject.SetActive(false);
    }

    private void ShowLevelUpPanel()
    {
        if (_levelUpPanel != null) _levelUpPanel.SetActive(true);
        if (_statsPanel != null) _statsPanel.SetActive(false);
        _levelUpDetail?.Initialize();
    }

    private void ShowStatsPanel()
    {
        if (_levelUpPanel != null) _levelUpPanel.SetActive(false);
        if (_statsPanel != null) _statsPanel.SetActive(true);
    }

    public void IntializeCurrency()
    {
        if (_txtCurrency == null || currencyManager == null) return;

        _txtCurrency.text = $"{currencyManager.Money}";
    }

    private void OnChangeCurrency()
    {
        IntializeCurrency();
    }

    public void InitializeStatsNode()
    {
        // Bersihkan dulu, kalau tidak setiap panel dibuka node-nya bertambah ganda.
        ClearNodes();

        if (playerBaseEntity == null || _statsNodePrefab == null || _statsNodeContainer == null) return;

        var stats = playerBaseEntity.AdditionalStats;
        if (stats == null) return;

        foreach (var stat in stats)
        {
            var statNode = Instantiate(_statsNodePrefab, _statsNodeContainer);
            statNode.Initialize(stat);
            statNode.OnSelected += HandleNodeSelected;
            statNode.OnUpgraded += HandleNodeUpgraded;
            nodes.Add(statNode);
        }

        if (nodes.Count > 0)
            HandleNodeSelected(nodes[0]);
    }

    private void HandleNodeSelected(StatsNodeUI selected)
    {
        selectedNode = selected;
        _statsDetailUI?.Show(selected != null ? selected.Stats : null);
    }

    // Setelah satu upgrade, node lain disegarkan (node sendiri sudah menyegarkan diri; uang juga memicu OnChange).
    private void HandleNodeUpgraded(StatsNodeUI upgradedNode)
    {
        RefreshAllNodes();

        if (selectedNode == upgradedNode)
            _statsDetailUI?.Show(upgradedNode.Stats);
    }

    public void RefreshAllNodes()
    {
        foreach (var node in nodes)
        {
            if (node != null) node.Refresh();
        }
    }

    private void ClearNodes()
    {
        foreach (var node in nodes)
        {
            if (node == null) continue;

            node.OnSelected -= HandleNodeSelected;
            node.OnUpgraded -= HandleNodeUpgraded;
            Destroy(node.gameObject);
        }

        nodes.Clear();
        selectedNode = null;
        _statsDetailUI?.Clear();
    }
}