using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreparationUI : MonoBehaviour
{
    [Header("References")]
    [Header("Night Counter")]
    [SerializeField] private TextMeshProUGUI _txtNightCounter;

    [Header("Button")]
    [SerializeField] private Button btnPreparation;
    [SerializeField] private Button btnExploreWorld;
    [SerializeField] private Button btnStartNight;

    [Header("Prepare Panel")]
    [SerializeField] private PreparePanel preparePanel;

    void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        HandleNightCounter();
        HandleNavigationButton();
    }

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf() => gameObject.SetActive(false);

    public void ActivatePreparePanel() => preparePanel.ActivateSelf();
    public void DeactivatePreparePanel() => preparePanel.DeactivateSelf();


    #region Helper

    private void HandleNightCounter()
    {
        if (_txtNightCounter == null) return;

        var nightManager = GameManager.Instance.nightController;

        _txtNightCounter.text = $"{nightManager.CurrentNightIndex + 1}";
    }

    private void HandleNavigationButton()
    {
        if (btnPreparation == null || btnExploreWorld == null || btnStartNight == null) return;

        btnPreparation.onClick.AddListener(ActivatePreparePanel);
        btnExploreWorld.onClick.AddListener(DeactivateSelf);
    }

    #endregion
}
