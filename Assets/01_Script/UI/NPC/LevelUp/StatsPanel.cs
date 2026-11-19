using UnityEngine;

public class StatsPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _statsNodeContainer;
    [SerializeField] private StatsNodeUI _statsNodePrefab;

    [Header("Detail Panel")]
    [SerializeField] private StatsDetailPanel _statsDetailPanel;

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf() => gameObject.SetActive(false);

    private void OnEnable()
    {
        if (_statsDetailPanel != null)
            _statsDetailPanel.OnStatUpgraded += HandleStatUpgraded;

        Initialize();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (_statsDetailPanel != null)
            _statsDetailPanel.OnStatUpgraded -= HandleStatUpgraded;
    }

    public void Initialize()
    {
        Unsubscribe();
        GenerateStatsNodes();
    }

    private void Unsubscribe()
    {
        if (_statsNodeContainer == null) return;

        foreach (Transform child in _statsNodeContainer)
        {
            var node = child.GetComponent<StatsNodeUI>();
            if (node != null)
                node.OnSelected -= HandleStatSelected;
        }
    }

    private void GenerateStatsNodes()
    {
        if (_statsNodeContainer == null || _statsNodePrefab == null)
        {
            ShowStatDetails(null);
            return;
        }

        ClearStatsNodes();

        var player = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
        var statsList = player?.AdditionalStats;
        if (statsList == null || statsList.Count == 0)
        {
            ShowStatDetails(null);
            return;
        }

        AdditionalStatsSO firstStat = null;
        AdditionalStatsSO vitalityStat = null;

        foreach (var stat in statsList)
        {
            if (stat == null) continue;

            var node = Instantiate(_statsNodePrefab, _statsNodeContainer);
            node.Initialize(stat);
            node.OnSelected += HandleStatSelected;

            if (firstStat == null)
                firstStat = stat;

            if (vitalityStat == null && stat.Type == StatsType.Vitality)
                vitalityStat = stat;
        }

        ShowStatDetails(vitalityStat != null ? vitalityStat : firstStat);
    }

    private void HandleStatSelected(StatsNodeUI node)
    {
        if (node != null)
            ShowStatDetails(node.Stats);
    }

    private void HandleStatUpgraded(AdditionalStatsSO upgradedStat)
    {
        if (_statsNodeContainer == null) return;

        foreach (Transform child in _statsNodeContainer)
        {
            var node = child.GetComponent<StatsNodeUI>();
            if (node != null && node.Stats == upgradedStat)
                node.Refresh();
        }
    }

    private void ShowStatDetails(AdditionalStatsSO stat)
    {
        if (_statsDetailPanel == null) return;

        if (stat == null)
        {
            _statsDetailPanel.Initialize(null);
            _statsDetailPanel.DeactivateSelf();
            return;
        }

        _statsDetailPanel.Initialize(stat);
        _statsDetailPanel.ActivateSelf();
    }

    private void ClearStatsNodes()
    {
        if (_statsNodeContainer == null) return;

        foreach (Transform child in _statsNodeContainer)
            Destroy(child.gameObject);
    }
}