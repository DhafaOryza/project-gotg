using System.Collections;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    [SerializeField] private Animator _animator;
    [SerializeField] private EnemyBaseEntity _enemyBaseEntity;
    [SerializeField] private float _movementThreshold = 0.1f;
    [SerializeField] private string _deathStateName = "Death";
    [SerializeField, Min(0f)] private float _deathStateEnterTimeout = 1f;

    private Rigidbody _enemyRigidbody;
    private bool _hasWalkingParameter;
    private bool _hasAttackParameter;
    private bool _hasDeathParameter;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_enemyBaseEntity == null)
            _enemyBaseEntity = GetComponentInParent<EnemyBaseEntity>();

        if (_enemyBaseEntity != null)
            _enemyRigidbody = _enemyBaseEntity.GetComponent<Rigidbody>();

        CacheParameters();
    }

    private void Update()
    {
        if (_animator == null || _enemyBaseEntity == null || _enemyRigidbody == null)
            return;

        if (!_hasWalkingParameter) return;

        bool isWalking = !_enemyBaseEntity.IsDead &&
            Mathf.Abs(_enemyRigidbody.linearVelocity.x) > _movementThreshold;

        _animator.SetBool(IsWalkingHash, isWalking);
    }

    /// <summary>
    /// Starts the attack animation. Returns false if there is no Attack trigger,
    /// so the caller can deal the damage right away instead.
    /// </summary>
    public bool PlayAttack()
    {
        if (_animator == null || !_hasAttackParameter)
            return false;

        _animator.ResetTrigger(AttackHash);
        _animator.SetTrigger(AttackHash);
        return true;
    }

    /// <summary>
    /// Starts the death animation. Returns false when the Animator has no Death trigger.
    /// </summary>
    public bool PlayDeath()
    {
        if (_animator == null || !_hasDeathParameter)
            return false;

        if (_hasWalkingParameter)
            _animator.SetBool(IsWalkingHash, false);

        _animator.ResetTrigger(DeathHash);
        _animator.SetTrigger(DeathHash);
        return true;
    }

    /// <summary>
    /// Waits until the configured death state has played through once.
    /// </summary>
    public IEnumerator WaitForDeathAnimation()
    {
        if (_animator == null || string.IsNullOrEmpty(_deathStateName))
            yield break;

        float enterTimer = 0f;
        bool enteredDeathState = false;

        while (!enteredDeathState && enterTimer < _deathStateEnterTimeout)
        {
            enteredDeathState = IsDeathStateCurrentOrNext();
            if (enteredDeathState) break;

            enterTimer += Time.deltaTime;
            yield return null;
        }

        if (!enteredDeathState)
        {
            Debug.LogWarning($"[{name}] Animator tidak masuk ke state '{_deathStateName}'.", this);
            yield break;
        }

        bool beganPlayingDeathState = false;
        while (true)
        {
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
            bool isCurrentDeathState = currentState.IsName(_deathStateName);

            if (isCurrentDeathState)
            {
                beganPlayingDeathState = true;
                if (currentState.normalizedTime >= 1f && !_animator.IsInTransition(0))
                    yield break;
            }
            else if (beganPlayingDeathState)
            {
                yield break;
            }

            yield return null;
        }
    }

    private bool IsDeathStateCurrentOrNext()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_deathStateName))
            return true;

        return _animator.IsInTransition(0) &&
               _animator.GetNextAnimatorStateInfo(0).IsName(_deathStateName);
    }

    /// <summary>
    /// Clears leftover animator values. Call this when the enemy comes out of the pool.
    /// </summary>
    public void ResetAnimation()
    {
        if (_animator == null) return;

        if (_hasAttackParameter) _animator.ResetTrigger(AttackHash);
        if (_hasDeathParameter) _animator.ResetTrigger(DeathHash);
        if (_hasWalkingParameter) _animator.SetBool(IsWalkingHash, false);
    }

    /// <summary>
    /// Called by the Animation Event placed on the hit frame of the attack clip.
    /// </summary>
    public void OnAttackAnimationEvent()
    {
        if (_enemyBaseEntity == null)
            _enemyBaseEntity = GetComponentInParent<EnemyBaseEntity>();

        if (_enemyBaseEntity != null)
            _enemyBaseEntity.ExecutePendingAttackFromAnimationEvent();
    }

    // Animator.parameters creates a new array on every call, so it is read once here.
    private void CacheParameters()
    {
        if (_animator == null) return;

        foreach (var parameter in _animator.parameters)
        {
            if (parameter.nameHash == IsWalkingHash && parameter.type == AnimatorControllerParameterType.Bool)
                _hasWalkingParameter = true;

            if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttackParameter = true;

            if (parameter.nameHash == DeathHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasDeathParameter = true;
        }
    }
}
