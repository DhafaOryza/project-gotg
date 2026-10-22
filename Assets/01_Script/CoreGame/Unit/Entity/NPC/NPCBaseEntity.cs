using System.Collections;
using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float _interactionRadius = 2f;

    [Header("Movement & Wandering Settings")]
    [SerializeField] private bool _canWander = true;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _wanderRadius = 5f;
    [SerializeField] private float _minWaitTime = 2f;
    [SerializeField] private float _maxWaitTime = 5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _isMoving;
    private bool _isInteracting;
    private Coroutine _wanderCoroutine;
    private Rigidbody _rigidbody;

    public float InteractionRadius => _interactionRadius;
    public bool CanWander => _canWander;

    protected NPCDataSO _npcData;

    protected override void Awake()
    {
        base.Awake();
        _npcData = entityData as NPCDataSO;
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        _startPosition = transform.position;

        if (_canWander)
        {
            StartWandering();
        }
    }

    /// <summary>
    /// Handles player interaction with the NPC, initiating dialogue and stopping movement.
    /// </summary>
    public virtual void Interact(PlayerBaseEntity player)
    {
        if (_npcData != null && !_npcData.IsInteractable)
        {
            return;
        }

        _isInteracting = true;
        _isMoving = false;

        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _wanderCoroutine = null;
        }

        ConversationUI conversationUI = PlayerUI.Instance != null ? PlayerUI.Instance.ConversationUI : null;
        if (conversationUI == null)
        {
            conversationUI = Object.FindFirstObjectByType<ConversationUI>();
        }
        
        if (conversationUI != null && _npcData != null)
        {
            conversationUI.StartNPCDialog(_npcData, OnConversationEnded);
        }
    }

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
            PlayerUI.Instance.HandleNpcRolePanel(_npcData, this);
        }
        else
        {
            ResumeWandering();
        }
    }

    /// <summary>
    /// Starts the wandering coroutine loop.
    /// </summary>
    private void StartWandering()
    {
        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
        }

        _wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    /// <summary>
    /// Resumes the NPC wandering behavior when panels close or interaction ends.
    /// </summary>
    public void ResumeWandering()
    {
        _isInteracting = false;
        if (_canWander)
        {
            StartWandering();
        }
    }

    /// <summary>
    /// Coroutine handling random horizontal wandering movement within a defined radius.
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!_isInteracting)
            {
                // Generate a random horizontal offset only (X-axis) while keeping Y and Z fixed
                float randomX = Random.Range(-_wanderRadius, _wanderRadius);
                _targetPosition = new Vector3(_startPosition.x + randomX, _startPosition.y, _startPosition.z);

                _isMoving = true;

                while (_isMoving && !_isInteracting && Mathf.Abs(transform.position.x - _targetPosition.x) > 0.1f)
                {
                    if (_rigidbody != null && _rigidbody.isKinematic)
                    {
                        Vector3 newPos = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
                        _rigidbody.MovePosition(newPos);
                    }
                    else
                    {
                        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
                    }

                    yield return null;
                }

                _isMoving = false;

                float waitTime = Random.Range(_minWaitTime, _maxWaitTime);
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);

        // Draw horizontal wander line/range representation in Gizmos
        Gizmos.color = Color.cyan;
        Vector3 centerPoint = Application.isPlaying ? _startPosition : transform.position;
        Vector3 leftLimit = centerPoint + Vector3.left * _wanderRadius;
        Vector3 rightLimit = centerPoint + Vector3.right * _wanderRadius;
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawWireSphere(leftLimit, 0.2f);
        Gizmos.DrawWireSphere(rightLimit, 0.2f);
    }
}