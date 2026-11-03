using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ConversationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _dialogPanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Image _portraitImage;

    [Header("Controller Input")]
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private InputSystemUIInputModule _uiInputModule;

    [Header("Typewriter Settings")]
    [SerializeField] private float _typingSpeed = 0.03f;

    private string[] _currentLines;
    private int _currentLineIndex;
    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private Action _onDialogCompletedCallback;
    private bool _previousNavigationEvents;
    private GameObject _previousFirstSelected;
    private bool _navigationEventsSuppressed;
    private bool _dialogInputPrepared;

    public bool IsDialogOpen => isActiveAndEnabled && _dialogPanel != null && _dialogPanel.activeInHierarchy;

    private void Update()
    {
        if (gameObject.activeSelf && _dialogPanel != null && _dialogPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            OnContinueClicked();
        }
    }

    private void OnDisable()
    {
        RestoreUIInputNavigation();
    }

    public void StartDialog(string speakerName, Sprite speakerPortrait, string[] lines, Action onComplete = null)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        SuppressUIInputNavigation();

        _currentLines = lines;
        _currentLineIndex = 0;
        _onDialogCompletedCallback = onComplete;

        if (_nameText != null) _nameText.text = speakerName;

        if (_portraitImage != null)
        {
            _portraitImage.gameObject.SetActive(speakerPortrait != null);
            if (speakerPortrait != null) _portraitImage.sprite = speakerPortrait;
        }

        if (_dialogPanel != null) _dialogPanel.SetActive(true);

        ShowCurrentLine();
    }

    private void SuppressUIInputNavigation()
    {
        if (_dialogInputPrepared)
        {
            if (_eventSystem != null)
                _eventSystem.SetSelectedGameObject(null);
            return;
        }

        _dialogInputPrepared = true;

        if (_eventSystem == null)
            _eventSystem = EventSystem.current != null
                ? EventSystem.current
                : FindFirstObjectByType<EventSystem>();

        if (_uiInputModule == null && _eventSystem != null)
            _uiInputModule = _eventSystem.GetComponent<InputSystemUIInputModule>();

        if (_uiInputModule != null && !_navigationEventsSuppressed)
        {
            _previousNavigationEvents = _uiInputModule.sendNavigationEvents;
            _uiInputModule.sendNavigationEvents = false;
            _navigationEventsSuppressed = true;
        }

        if (_eventSystem != null)
        {
            _previousFirstSelected = _eventSystem.firstSelectedGameObject;
            _eventSystem.firstSelectedGameObject = null;
            _eventSystem.SetSelectedGameObject(null);
        }
    }

    private void RestoreUIInputNavigation()
    {
        if (_navigationEventsSuppressed && _uiInputModule != null)
            _uiInputModule.sendNavigationEvents = _previousNavigationEvents;

        _navigationEventsSuppressed = false;
        _dialogInputPrepared = false;

        if (_eventSystem != null)
        {
            _eventSystem.firstSelectedGameObject =
                _previousFirstSelected != null && _previousFirstSelected.activeInHierarchy
                    ? _previousFirstSelected
                    : null;
        }

        _previousFirstSelected = null;
    }

    public void StartNPCDialog(NPCDataSO npcData, Action onComplete = null)
    {
        if (npcData == null) return;
        StartDialog(npcData.NpcName, npcData.NpcPortrait, npcData.DialogueLines, onComplete);
    }

    private void ShowCurrentLine()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeLineRoutine(_currentLines[_currentLineIndex]));
    }

    private IEnumerator TypeLineRoutine(string line)
    {
        _isTyping = true;
        _dialogueText.text = "";

        foreach (char c in line.ToCharArray())
        {
            _dialogueText.text += c;
            yield return new WaitForSecondsRealtime(_typingSpeed);
        }

        _isTyping = false;
    }

    public void OnContinueClicked()
    {
        if (_isTyping)
        {
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _dialogueText.text = _currentLines[_currentLineIndex];
            _isTyping = false;
            return;
        }

        _currentLineIndex++;

        if (_currentLineIndex < _currentLines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialog();
        }
    }

    private void EndDialog()
    {
        if (_dialogPanel != null) _dialogPanel.SetActive(false);

        RestoreUIInputNavigation();

        // Panggil callback untuk melanjutkan proses setelah dialog selesai
        Action callback = _onDialogCompletedCallback;
        _onDialogCompletedCallback = null;
        callback?.Invoke();
    }
}
