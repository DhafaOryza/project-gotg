using TMPro;
using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject _interactSign;
    [SerializeField] private TextMeshPro _txtSign;
    [SerializeField] private TextMeshPro _txtCharacterName;

    [Header("Interaction Settings")]
    [SerializeField] private float _interactionRadius = 2f;

    public bool isInteractable = true;

    private NPCMovement _npcMovement;
    private NPCDataSO _npcData => entityData as NPCDataSO;

    private PlayerBaseEntity _player;
    private bool signVisible;
    private bool signSuppressed;

    public event System.Action OnInteract;

    void OnEnable()
    {
        OnInteract += HandleNpcConversation;
    }

    void Onestroy()
    {
        OnInteract -= HandleNpcConversation;
    }

    protected override void Awake()
    {
        base.Awake();
        _npcMovement = GetComponent<NPCMovement>();
    }

    void Start()
    {
        if (_interactSign != null) _interactSign.SetActive(false);
        signVisible = false;

        UpdateSignText();
        UpdateCharacterName();

        if (_player == null) _player = GameManager.Instance?.levelSpawnerManager.GetCurrentPlayer();
    }

    void Update()
    {
        if (_player == null)
        {
            SetSign(false);
            SetName(false);
            return;
        }

        bool inRange = IsPlayerInRange();

        if (!inRange)
            signSuppressed = false;

        bool shouldShow = isInteractable && _player.CanAct && inRange && !signSuppressed;
        SetSign(shouldShow);
        SetName(shouldShow);
    }

    #region Interact Detection

    private bool IsPlayerInRange()
    {
        // Pakai jarak X dan Y saja; sumbu Z diabaikan karena game bergerak di bidang 2D.
        Vector2 diff = (Vector2)_player.transform.position - (Vector2)transform.position;
        return diff.sqrMagnitude <= _interactionRadius * _interactionRadius;
    }

    private void SetSign(bool visible)
    {
        if (_interactSign == null) return;

        if (visible) UpdateSignText();

        _interactSign.SetActive(visible);
    }

    private void UpdateSignText()
    {
        if (_txtSign == null) return;

        var playerController = GameManager.Instance?.playerController;
        if (playerController == null) return;

        _txtSign.text = $"{playerController.interact}";
    }

    private void SetName(bool visible)
    {
        if (_txtCharacterName == null) return;

        if (visible) UpdateCharacterName();

        _txtCharacterName.gameObject.SetActive(visible);
    }

    private void UpdateCharacterName()
    {
        if (_txtCharacterName == null) return;

        var npcName = _npcData.NpcName;

        if (npcName == null) return;
        _txtCharacterName.text = $"{npcName}";
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;

        if (!value)
        {
            SetSign(false);
            SetName(false);
        }
    }

    public void ResumeSign()
    {
        signSuppressed = false;
    }
    /// <summary>
    /// Handles player interaction with the NPC, initiating dialogue and pausing movement.
    /// </summary>
    public virtual void Interact(PlayerBaseEntity player)
    {
        if (!isInteractable) return;

        signSuppressed = true;
        SetSign(false);
        SetName(false);

        OnInteract?.Invoke();

        if (_npcMovement != null)
        {
            _npcMovement.PauseMovement();
        }
    }

    private void HandleNpcConversation()
    {
        var conversationUI = PlayerUI.Instance?.ConversationUI;
        if (conversationUI == null) conversationUI = Object.FindFirstObjectByType<ConversationUI>();

        if (conversationUI == null && _npcData == null) return;

        conversationUI.StartNPCDialog(_npcData, OnConversationEnded);
    }

    #endregion

    /// <summary>
    /// Handles events after the conversation ends, requesting quests and opening role panels.
    /// </summary>
    protected virtual void OnConversationEnded()
    {
        if (_npcData != null && _npcData.AssignedQuest != null && GameManager.Instance?.questManager != null)
        {
            GameManager.Instance.questManager.RequestSpecificQuest(_npcData.AssignedQuest);
        }

        if (PlayerUI.Instance != null && _npcData != null)
        {
            PlayerUI.Instance.HandleNpcRolePanel(_npcData, _npcMovement);
        }
        else
        {
            ResumeMovement();
        }
    }

    /// <summary>
    /// Resumes the NPC movement behavior when panels close or interaction ends.
    /// </summary>
    public void ResumeMovement()
    {
        if (_npcMovement != null)
        {
            _npcMovement.ResumeMovement();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);

        if (_npcMovement != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 centerPoint = Application.isPlaying ? _npcMovement.StartPosition : transform.position;
            float radius = _npcMovement.WanderRadius;
            Vector3 leftLimit = centerPoint + Vector3.left * radius;
            Vector3 rightLimit = centerPoint + Vector3.right * radius;
            Gizmos.DrawLine(leftLimit, rightLimit);
            Gizmos.DrawWireSphere(leftLimit, 0.2f);
            Gizmos.DrawWireSphere(rightLimit, 0.2f);
        }
    }
}