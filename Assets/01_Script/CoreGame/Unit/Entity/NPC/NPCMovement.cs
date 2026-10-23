using System.Collections;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private NpcMovementType _movementType = NpcMovementType.WANDERING;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _wanderRadius = 5f;
    [SerializeField] private float _minWaitTime = 2f;
    [SerializeField] private float _maxWaitTime = 5f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _isMoving;
    private bool _isPaused;
    private Coroutine _wanderCoroutine;
    private Rigidbody _rigidbody;

    public NpcMovementType MovementType => _movementType;
    public bool IsPaused => _isPaused;
    public Vector3 StartPosition => _startPosition;
    public float WanderRadius => _wanderRadius;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _startPosition = transform.position;

        if (_movementType == NpcMovementType.WANDERING)
        {
            StartWandering();
        }
    }

    /// <summary>
    /// Starts the wandering coroutine loop.
    /// </summary>
    public void StartWandering()
    {
        if (_movementType != NpcMovementType.WANDERING) return;

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
        _isMoving = false;

        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _wanderCoroutine = null;
        }
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
}