using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles interactive keybind rebinding UI.
/// Each action gets a "Rebind" button and a label that shows the current binding.
/// Attach to the same GameObject as SettingsUI (or any persistent UI root).
/// </summary>
public class KeybindSettingsUI : MonoBehaviour
{
    [Header("Rebind Buttons")]
    [SerializeField] private Button _rebindMoveButton;
    [SerializeField] private Button _rebindSkill1Button;
    [SerializeField] private Button _rebindSkill2Button;
    [SerializeField] private Button _rebindSkill3Button;
    [SerializeField] private Button _rebindSkill4Button;
    [SerializeField] private Button _rebindInteractButton;
    [SerializeField] private Button _rebindCancelButton;
    [SerializeField] private Button _rebindResetAllButton;

    [Header("Binding Labels")]
    [SerializeField] private TextMeshProUGUI _moveLabel;
    [SerializeField] private TextMeshProUGUI _skill1Label;
    [SerializeField] private TextMeshProUGUI _skill2Label;
    [SerializeField] private TextMeshProUGUI _skill3Label;
    [SerializeField] private TextMeshProUGUI _skill4Label;
    [SerializeField] private TextMeshProUGUI _interactLabel;
    [SerializeField] private TextMeshProUGUI _cancelLabel;

    [Header("Rebind Prompt")]
    [SerializeField] private GameObject _rebindPromptPanel;
    [SerializeField] private TextMeshProUGUI _rebindPromptText;

    private InputActionRebindingExtensions.RebindingOperation _currentRebind;
    private string _pendingActionName;

    // Controls to exclude from rebinding (mouse movement, etc.)
    private static readonly string[] ExcludedPaths =
    {
        "<Mouse>/position",
        "<Mouse>/delta",
        "<Mouse>/scroll"
    };

    private void Awake()
    {
        _rebindMoveButton?.onClick.AddListener(() => StartRebind("Move", _moveLabel));
        _rebindSkill1Button?.onClick.AddListener(() => StartRebind("Skill 1", _skill1Label));
        _rebindSkill2Button?.onClick.AddListener(() => StartRebind("Skill 2", _skill2Label));
        _rebindSkill3Button?.onClick.AddListener(() => StartRebind("Skill 3", _skill3Label));
        _rebindSkill4Button?.onClick.AddListener(() => StartRebind("Skill 4", _skill4Label));
        _rebindInteractButton?.onClick.AddListener(() => StartRebind("Interact", _interactLabel));
        _rebindCancelButton?.onClick.AddListener(() => StartRebind("Cancel", _cancelLabel));
        _rebindResetAllButton?.onClick.AddListener(OnResetAllClicked);
    }

    private void OnDestroy()
    {
        CancelCurrentRebind();

        _rebindMoveButton?.onClick.RemoveAllListeners();
        _rebindSkill1Button?.onClick.RemoveAllListeners();
        _rebindSkill2Button?.onClick.RemoveAllListeners();
        _rebindSkill3Button?.onClick.RemoveAllListeners();
        _rebindSkill4Button?.onClick.RemoveAllListeners();
        _rebindInteractButton?.onClick.RemoveAllListeners();
        _rebindCancelButton?.onClick.RemoveAllListeners();
        _rebindResetAllButton?.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Call this when the Keybind tab is opened to refresh all labels.
    /// </summary>
    public void RefreshAllLabels()
    {
        RefreshLabel("Move", _moveLabel);
        RefreshLabel("Skill 1", _skill1Label);
        RefreshLabel("Skill 2", _skill2Label);
        RefreshLabel("Skill 3", _skill3Label);
        RefreshLabel("Skill 4", _skill4Label);
        RefreshLabel("Interact", _interactLabel);
        RefreshLabel("Cancel", _cancelLabel);
    }

    // ── Rebind Operation ──────────────────────────────────────────────────

    private void StartRebind(string actionName, TextMeshProUGUI displayLabel)
    {
        if (KeybindManager.Instance == null)
        {
            Debug.LogWarning("[KeybindSettingsUI] KeybindManager not found.");
            return;
        }

        var asset = KeybindManager.Instance.InputActionAsset;
        if (asset == null) return;

        var action = asset.FindAction(actionName, false);
        if (action == null)
        {
            Debug.LogWarning($"[KeybindSettingsUI] Action '{actionName}' not found.");
            return;
        }

        CancelCurrentRebind();

        _pendingActionName = actionName;
        ShowPrompt($"Press any key for {actionName}...");

        // Disable the action map so input doesn't trigger gameplay during rebind
        var actionMap = action.actionMap;
        bool wasEnabled = actionMap.enabled;
        if (wasEnabled) actionMap.Disable();

        var operation = action.PerformInteractiveRebinding()
            .WithControlsExcluding(ExcludedPaths[0])
            .WithControlsExcluding(ExcludedPaths[1])
            .WithControlsExcluding(ExcludedPaths[2])
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.2f)
            .OnCancel(op =>
            {
                HidePrompt();
                if (wasEnabled) actionMap.Enable();
                op.Dispose();
                _currentRebind = null;
                _pendingActionName = null;
            })
            .OnComplete(op =>
            {
                var control = op.selectedControl;
                if (control != null)
                {
                    string newPath = control.path;
                    KeybindManager.Instance.SetBinding(actionName, newPath);

                    if (displayLabel != null)
                    {
                        displayLabel.text = InputControlPath.ToHumanReadableString(
                            newPath,
                            InputControlPath.HumanReadableStringOptions.OmitDevice);
                    }
                }

                HidePrompt();
                if (wasEnabled) actionMap.Enable();
                op.Dispose();
                _currentRebind = null;
                _pendingActionName = null;
            })
            .Start();

        _currentRebind = operation;
    }

    private void CancelCurrentRebind()
    {
        _currentRebind?.Cancel();
        _currentRebind?.Dispose();
        _currentRebind = null;
        _pendingActionName = null;
        HidePrompt();
    }

    // ── Reset All ─────────────────────────────────────────────────────────

    private void OnResetAllClicked()
    {
        if (KeybindManager.Instance == null) return;

        KeybindManager.Instance.ResetAllBindings();
        RefreshAllLabels();
        Debug.Log("[KeybindSettingsUI] All keybinds reset to default.");
    }

    // ── Label Helpers ─────────────────────────────────────────────────────

    private void RefreshLabel(string actionName, TextMeshProUGUI label)
    {
        if (label == null || KeybindManager.Instance == null) return;

        var asset = KeybindManager.Instance.InputActionAsset;
        if (asset == null) return;

        var action = asset.FindAction(actionName, false);
        if (action == null || action.bindings.Count == 0) return;

        string path = action.bindings[0].effectivePath;

        label.text = InputControlPath.ToHumanReadableString(
            path,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    // ── Prompt UI ─────────────────────────────────────────────────────────

    private void ShowPrompt(string message)
    {
        if (_rebindPromptPanel != null) _rebindPromptPanel.SetActive(true);
        if (_rebindPromptText != null) _rebindPromptText.text = message;
    }

    private void HidePrompt()
    {
        if (_rebindPromptPanel != null) _rebindPromptPanel.SetActive(false);
    }
}