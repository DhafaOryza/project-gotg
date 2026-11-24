using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Settings screen: tab switching, control binding to SettingsData,
/// and Save/Back button wiring.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button _volumeTabButton;
    [SerializeField] private Button _graphicsTabButton;
    [SerializeField] private Button _keybindTabButton;

    [Header("Navigation")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Selectable _firstSelected;

    [Header("Tab Panels")]
    [SerializeField] private GameObject _volumePanel;
    [SerializeField] private GameObject _graphicsPanel;
    [SerializeField] private GameObject _keybindPanel;

    [Header("Volume Tab")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Graphics Tab")]
    [SerializeField] private TMP_Dropdown _fullscreenDropdown;
    [SerializeField] private Toggle _shakeToggle;

    private void Awake()
    {
        _volumeTabButton?.onClick.AddListener(() => ShowTab(0));
        _graphicsTabButton?.onClick.AddListener(() => ShowTab(1));
        _keybindTabButton?.onClick.AddListener(() => ShowTab(2));

        _musicSlider?.onValueChanged.AddListener(OnMusicChanged);
        _sfxSlider?.onValueChanged.AddListener(OnSFXChanged);
        _fullscreenDropdown?.onValueChanged.AddListener(OnFullscreenChanged);
        _shakeToggle?.onValueChanged.AddListener(OnShakeChanged);

        _backButton?.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        _musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
        _sfxSlider?.onValueChanged.RemoveListener(OnSFXChanged);
        _fullscreenDropdown?.onValueChanged.RemoveListener(OnFullscreenChanged);
        _shakeToggle?.onValueChanged.RemoveListener(OnShakeChanged);
        _backButton?.onClick.RemoveListener(OnBackClicked);
    }

    private void OnEnable()
    {
        RefreshFromData();
        ShowTab(0);

        if (_firstSelected != null)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            eventSystem?.SetSelectedGameObject(_firstSelected.gameObject);
        }
    }

    // ── Event Handlers (Auto-Save on Change) ──────────────────────────

    private void OnMusicChanged(float value)
    {
        SettingsData.Instance?.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        SettingsData.Instance?.SetSFXVolume(value);
    }

    private void OnFullscreenChanged(int index)
    {
        // Dropdown: 0 = Full-Screen, 1 = Windowed
        SettingsData.Instance?.SetFullscreen(index == 0);
    }

    private void OnShakeChanged(bool value)
    {
        SettingsData.Instance?.SetShake(value);
    }

    private void OnBackClicked()
    {
        // Final save to ensure all values are persisted
        PlayerPrefs.Save();

        gameObject.SetActive(false);
        MainMenuUI.Instance?.OnSettingsClosed();
    }

    // ── Tab Switching ─────────────────────────────────────────────────

    private void ShowTab(int index)
    {
        if (_volumePanel != null) _volumePanel.SetActive(index == 0);
        if (_graphicsPanel != null) _graphicsPanel.SetActive(index == 1);
        if (_keybindPanel != null) _keybindPanel.SetActive(index == 2);
    }

    // ── Sync Controls to Saved State ──────────────────────────────────

    private void RefreshFromData()
    {
        var data = SettingsData.Instance;
        if (data == null) return;

        if (_musicSlider != null)
        {
            _musicSlider.SetValueWithoutNotify(data.MusicVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.SetValueWithoutNotify(data.SFXVolume);
        }

        if (_fullscreenDropdown != null)
        {
            _fullscreenDropdown.SetValueWithoutNotify(data.IsFullscreen ? 0 : 1);
        }

        if (_shakeToggle != null)
        {
            _shakeToggle.SetIsOnWithoutNotify(data.ShakeEnabled);
        }
    }
}