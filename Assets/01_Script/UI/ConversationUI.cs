using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConversationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _dialogPanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Image _portraitImage;

    [Header("Typewriter Settings")]
    [SerializeField] private float _typingSpeed = 0.03f;

    private string[] _currentLines;
    private int _currentLineIndex;
    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private Action _onDialogCompletedCallback;

    private void OnEnable()
    {
        Initialize();
    }

    private void Update()
    {
        if (_dialogPanel != null && _dialogPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            OnContinueClicked();
        }
    }

    /// <summary>
    /// Initializes the conversation UI components on enable.
    /// </summary>
    private void Initialize()
    {
        if (_dialogPanel != null)
        {
            _dialogPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Starts a general dialogue session with speaker info and lines.
    /// </summary>
    public void StartDialog(string speakerName, Sprite speakerPortrait, string[] lines, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _currentLines = lines;
        _currentLineIndex = 0;
        _onDialogCompletedCallback = onComplete;

        if (_nameText != null) _nameText.text = speakerName;

        if (_portraitImage != null)
        {
            if (speakerPortrait != null)
            {
                _portraitImage.sprite = speakerPortrait;
                _portraitImage.gameObject.SetActive(true);
            }
            else
            {
                _portraitImage.gameObject.SetActive(false);
            }
        }

        if (_dialogPanel != null)
            _dialogPanel.SetActive(true);

        ShowCurrentLine();
    }

    /// <summary>
    /// Starts dialogue session specifically using NPCDataSO properties.
    /// </summary>
    public void StartNPCDialog(NPCDataSO npcData, Action onComplete = null)
    {
        if (npcData == null) return;
        StartDialog(npcData.NpcName, npcData.NpcPortrait, npcData.DialogueLines, onComplete);
    }

    private void ShowCurrentLine()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

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

    /// <summary>
    /// Handles progression to the next line or skips the typewriter effect on click.
    /// </summary>
    public void OnContinueClicked()
    {
        if (_isTyping)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

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
        if (_dialogPanel != null)
            _dialogPanel.SetActive(false);

        Action callback = _onDialogCompletedCallback;
        _onDialogCompletedCallback = null;
        callback?.Invoke();
    }
}