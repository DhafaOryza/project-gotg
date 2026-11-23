using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles Main Menu button navigation: Start, Settings, Credits, Quit.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [Header("Main Content")]
    [SerializeField] private GameObject _mainContent;

    [Header("Settings")]
    [SerializeField] private GameObject _settingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _creditButton;
    [SerializeField] private Button _quitButton;

    [Header("Scene")]
    [SerializeField] private string _gameSceneName = "Gameplay";

    [Header("First Selected")]
    [SerializeField] private Selectable _firstSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        _startButton?.onClick.AddListener(OnStartClicked);
        _settingsButton?.onClick.AddListener(OnSettingsClicked);
        _creditButton?.onClick.AddListener(OnCreditClicked);
        _quitButton?.onClick.AddListener(OnQuitClicked);

        ShowMainContent(true);
        FocusFirst();
    }

    // ── Button handlers ───────────────────────────────────────────────────

    private void OnStartClicked()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    private void OnSettingsClicked()
    {
        ShowMainContent(false);

        if (_settingsPanel != null)
            _settingsPanel.SetActive(true);
    }

    private void OnCreditClicked()
    {
        // TODO: buka panel credit
        Debug.Log("[MainMenuUI] Credit belum diimplementasikan.");
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Called by SettingsUI when Back is pressed ─────────────────────────

    public void OnSettingsClosed()
    {
        ShowMainContent(true);
        FocusFirst();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void ShowMainContent(bool show)
    {
        if (_mainContent != null)
            _mainContent.SetActive(show);
    }

    private void FocusFirst()
    {
        if (_firstSelected == null) return;

        var es = EventSystem.current;
        if (es != null)
            es.SetSelectedGameObject(_firstSelected.gameObject);
    }
}