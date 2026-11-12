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

    [Header("Navigation")]
    [SerializeField] private Selectable firstSelectedButton;

    public Selectable FirstSelectedButton => firstSelectedButton;
    public bool IsPreparePanelOpen => preparePanel != null && preparePanel.gameObject.activeInHierarchy;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (btnPreparation != null)
            btnPreparation.onClick.RemoveListener(ActivatePreparePanel);

        if (btnExploreWorld != null)
            btnExploreWorld.onClick.RemoveListener(HandleExploreWorldButton);
    }

    public void Initialize()
    {
        HandleNightCounter();
        HandleNavigationButton();

        DeactivatePreparePanel();
    }

    public void ActivateSelf() => gameObject.SetActive(true);
    public void DeactivateSelf() => gameObject.SetActive(false);

    public void ActivatePreparePanel()
    {
        if (preparePanel != null)
            preparePanel.ActivateSelf();
    }

    public void DeactivatePreparePanel()
    {
        if (preparePanel != null)
            preparePanel.DeactivateSelf();
    }


    #region Helper

    private void HandleNightCounter()
    {
        if (_txtNightCounter == null) return;

        var nightManager = GameManager.Instance?.nightController;
        if (nightManager == null) return;

        _txtNightCounter.text = $"Night {nightManager.CurrentNightIndex + 1}";
    }

    private void HandleNavigationButton()
    {
        if (btnPreparation != null)
        {
            btnPreparation.onClick.RemoveListener(ActivatePreparePanel);
            btnPreparation.onClick.AddListener(ActivatePreparePanel);
        }

        if (btnExploreWorld != null)
        {
            btnExploreWorld.onClick.RemoveListener(HandleExploreWorldButton);
            btnExploreWorld.onClick.AddListener(HandleExploreWorldButton);
        }
    }

    private void HandleExploreWorldButton()
    {
        if (PlayerUI.Instance != null)
            PlayerUI.Instance.DeactivatePreparationUI();
        else
            DeactivateSelf();
    }

    #endregion
}
