using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderPanelUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _levelUpTabButton;
    [SerializeField] private Button _statsTabButton;

    [Header("Panels")]
    [SerializeField] private GameObject _levelUpPanel;
    [SerializeField] private GameObject _statsPanel;

    [Header("Level Up")]
    [SerializeField] private BuilderLevelUpDetail _levelUpDetail;

    [Header("Repair")]
    [SerializeField] private BuilderRepairDetail _repairDetail;

    [Header("Stats")]
    [SerializeField] private Transform _statsNodeContainer;
    [SerializeField] private BuilderStatsNodeUI _statsNodePrefab;
    [SerializeField] private BuilderStatsDetailUI _statsDetailUI;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _txtCurrency;

    [Header("Granary")]
    [SerializeField] private GranaryBase _granary;

    private CurrencyManager _currencyManager;
    private readonly List<BuilderStatsNodeUI> _nodes = new List<BuilderStatsNodeUI>();
    private BuilderStatsNodeUI _selectedNode;

    private void Awake()
    {
        if (_backButton != null) _backButton.onClick.AddListener(CloseUI);
        if (_levelUpTabButton != null) _levelUpTabButton.onClick.AddListener(ShowLevelUpPanel);
        if (_statsTabButton != null) _statsTabButton.onClick.AddListener(ShowStatsPanel);
    }

    private void OnDestroy()
    {
        if (_backButton != null) _backButton.onClick.RemoveListener(CloseUI);
        if (_levelUpTabButton != null) _levelUpTabButton.onClick.RemoveListener(ShowLevelUpPanel);
        if (_statsTabButton != null) _statsTabButton.onClick.RemoveListener(ShowStatsPanel);
    }

    private void OnEnable()
    {
        // The manager must be fetched BEFORE subscribing, it can still be null on the first open.
        _currencyManager = GameManager.Instance != null ? GameManager.Instance.currencyManager : null;
        if (_currencyManager != null)
            _currencyManager.OnChange += RefreshCurrency;

        if (_granary == null)
            _granary = FindAnyObjectByType<GranaryBase>();

        Initialize();
    }

    private void OnDisable()
    {
        if (_currencyManager != null)
            _currencyManager.OnChange -= RefreshCurrency;

        ClearNodes();
    }

    public void Initialize()
    {
        InitializeStatsNodes();
        RefreshCurrency();

        // Repair is set up here, not in ShowLevelUpPanel, so it works whichever tab is open.
        if (_repairDetail != null) _repairDetail.Initialize(_granary);

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

        if (_levelUpDetail != null) _levelUpDetail.Initialize(_granary);
    }

    private void ShowStatsPanel()
    {
        if (_levelUpPanel != null) _levelUpPanel.SetActive(false);
        if (_statsPanel != null) _statsPanel.SetActive(true);
    }

    private void RefreshCurrency()
    {
        if (_txtCurrency == null || _currencyManager == null) return;

        _txtCurrency.text = $"{_currencyManager.Money}";
    }

    #region Stats Nodes

    private void InitializeStatsNodes()
    {
        // Clear first, otherwise the nodes are duplicated every time the panel opens.
        ClearNodes();

        if (_granary == null || _statsNodePrefab == null || _statsNodeContainer == null) return;

        var stats = _granary.AdditionalStats;
        if (stats == null) return;

        foreach (var stat in stats)
        {
            BuilderStatsNodeUI node = Instantiate(_statsNodePrefab, _statsNodeContainer);
            node.Initialize(stat, _granary);
            node.OnSelected += HandleNodeSelected;
            node.OnUpgraded += HandleNodeUpgraded;
            _nodes.Add(node);
        }

        if (_nodes.Count > 0)
            HandleNodeSelected(_nodes[0]);
    }

    private void HandleNodeSelected(BuilderStatsNodeUI selected)
    {
        _selectedNode = selected;

        if (_statsDetailUI != null)
            _statsDetailUI.Show(selected != null ? selected.Stats : null);
    }

    private void HandleNodeUpgraded(BuilderStatsNodeUI upgradedNode)
    {
        foreach (var node in _nodes)
        {
            if (node != null) node.Refresh();
        }

        if (_selectedNode == upgradedNode && _statsDetailUI != null)
            _statsDetailUI.Show(upgradedNode.Stats);
    }

    private void ClearNodes()
    {
        foreach (var node in _nodes)
        {
            if (node == null) continue;

            node.OnSelected -= HandleNodeSelected;
            node.OnUpgraded -= HandleNodeUpgraded;
            Destroy(node.gameObject);
        }

        _nodes.Clear();
        _selectedNode = null;

        if (_statsDetailUI != null)
            _statsDetailUI.Clear();
    }

    #endregion
}