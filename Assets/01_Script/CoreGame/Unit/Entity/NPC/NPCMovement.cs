using System.Collections;
using UnityEngine;

public class NPCMovement : MonoBehaviour, IPoolable
{
    [SerializeField] private NpcMovementType _movementType = NpcMovementType.WANDERING;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _wanderRadius = 5f;
    [SerializeField] private float _minWaitTime = 2f;
    [SerializeField] private float _maxWaitTime = 5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _hasStartPosition;
    private bool _isMoving;
    private bool _isPaused;
    private Coroutine _wanderCoroutine;
    private Rigidbody _rigidbody;

    public NpcMovementType MovementType => _movementType;
    public bool IsPaused => _isPaused;
    public Vector3 StartPosition => _startPosition;
    public float WanderRadius => _wanderRadius;

    // NPC Animations
    public bool IsMoving => _isMoving;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // NPCs placed directly in the scene are not pooled, so OnSpawn never runs for them.
        if (!_hasStartPosition)
            ResetMovement();
    }

    #region Pool

    /// <summary>
    /// Called by the pool when this NPC is taken out of the pool. Starts movement from a clean state.
    /// </summary>
    public void OnSpawn()
    {
        ResetMovement();
    }

    /// <summary>
    /// Called by the pool right before this NPC is put back into the pool.
    /// </summary>
    public void OnDespawn()
    {
        StopWandering();
    }

    /// <summary>
    /// Clears any leftover state, takes the current position as the wander center and starts wandering.
    /// </summary>
    private void ResetMovement()
    {
        StopWandering();

        _startPosition = transform.position;
        _hasStartPosition = true;
        _isPaused = false;

        if (_movementType == NpcMovementType.WANDERING)
            StartWandering();
    }

    #endregion

    /// <summary>
    /// Starts the wandering coroutine loop.
    /// </summary>
    public void StartWandering()
    {
        if (_movementType != NpcMovementType.WANDERING) return;

        // A coroutine cannot start on an inactive object. OnSpawn starts it again on the next spawn.
        if (!isActiveAndEnabled) return;

        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
        }

        _wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    /// <summary>
    /// Pauses the NPC movement during interaction or events.
    /// </summary>
    public void PauseMovement()
    {
        _isPaused = true;
        StopWandering();
    }

    /// <summary>
    /// Resumes the NPC wandering behavior if movement type allows.
    /// </summary>
    public void ResumeMovement()
    {
        _isPaused = false;
        if (_movementType == NpcMovementType.WANDERING)
        {
            StartWandering();
        }
    }

    private void StopWandering()
    {
        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _wanderCoroutine = null;
        }

        _isMoving = false;
    }

    /// <summary>
    /// Coroutine handling random horizontal wandering movement within a defined radius.
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!_isPaused && _movementType == NpcMovementType.WANDERING)
            {
                float randomX = Random.Range(-_wanderRadius, _wanderRadius);
                _targetPosition = new Vector3(_startPosition.x + randomX, _startPosition.y, _startPosition.z);

                FlipDirection(_targetPosition.x - transform.position.x);

                _isMoving = true;

                while (_isMoving && !_isPaused && Mathf.Abs(transform.position.x - _targetPosition.x) > 0.1f)
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

    /// <summary>
    /// Flips the character's facing direction based on movement direction (X-axis).
    /// </summary>
    private void FlipDirection(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            Vector3 localScale = transform.localScale;
            if (directionX > 0)
            {
                localScale.x = -Mathf.Abs(localScale.x);
            }
            else
            {
                localScale.x = Mathf.Abs(localScale.x);
            }
            transform.localScale = localScale;
        }
    }
}