using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsKnockDownHash = Animator.StringToHash("IsKnockDown");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");

    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerBaseEntity _playerBaseEntity;
    [FormerlySerializedAs("movementThreshold")]
    [SerializeField] private float _movementThreshold = 0.1f;

    private Rigidbody _playerRigidbody;
    private bool _hasWalkingParameter;
    private bool _hasKnockDownParameter;
    private bool _hasAttackParameter;
    private bool _hasHurtParameter;

    public Animator Animator => _animator;
    public PlayerBaseEntity PlayerBaseEntity => _playerBaseEntity;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_playerBaseEntity == null)
            _playerBaseEntity = GetComponentInParent<PlayerBaseEntity>();

        if (_playerBaseEntity != null)
            _playerRigidbody = _playerBaseEntity.GetComponent<Rigidbody>();

        CacheParameters();
    }

    private void OnEnable()
    {
        if (_playerBaseEntity != null)
            _playerBaseEntity.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (_playerBaseEntity != null)
            _playerBaseEntity.OnDamaged -= HandleDamaged;
    }

    private void Update()
    {
        if (_animator == null || _playerBaseEntity == null || _playerRigidbody == null)
            return;

        bool isKnockDown = _playerBaseEntity.IsKnockDown;

        if (_hasWalkingParameter)
        {
            bool isWalking = _playerBaseEntity.CanAct &&
                Mathf.Abs(_playerRigidbody.linearVelocity.x) > _movementThreshold;

            _animator.SetBool(IsWalkingHash, isWalking);
        }

        if (_hasKnockDownParameter)
            _animator.SetBool(IsKnockDownHash, isKnockDown);

        // Old triggers would play right after the revive, so they are cleared while knocked down.
        if (isKnockDown)
            ClearTriggers();
    }

    /// <summary>
    /// Starts the attack animation. Returns false if there is no Attack trigger,
    /// so the caller can run the skill right away instead.
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
    /// Plays the hurt animation. It is skipped while an attack still waits for its Animation Event,
    /// otherwise the hurt animation would cut the attack and the skill would only fire late via the fallback.
    /// </summary>
    public bool PlayHurt()
    {
        if (_animator == null || !_hasHurtParameter)
            return false;

        if (_playerBaseEntity != null && _playerBaseEntity.HasPendingSkill)
            return false;

        _animator.ResetTrigger(HurtHash);
        _animator.SetTrigger(HurtHash);
        return true;
    }

    /// <summary>
    /// Called by the Animation Event placed on the hit frame of the Atk clip.
    /// </summary>
    public void OnAttackAnimationEvent()
    {        
        if (_playerBaseEntity == null)
            _playerBaseEntity = GetComponentInParent<PlayerBaseEntity>();

        _playerBaseEntity?.ExecutePendingSkillFromAnimationEvent();
    }

    private void HandleDamaged(int amount)
    {
        // A fatal hit goes straight to the KO animation, so no hurt animation for it.
        if (_playerBaseEntity.CurrentHealth <= 0) return;

        PlayHurt();
    }

    private void ClearTriggers()
    {
        if (_hasAttackParameter) _animator.ResetTrigger(AttackHash);
        if (_hasHurtParameter) _animator.ResetTrigger(HurtHash);
    }

    // Animator.parameters creates a new array on every call, so it is read once here.
    private void CacheParameters()
    {
        if (_animator == null) return;

        foreach (var parameter in _animator.parameters)
        {
            if (parameter.nameHash == IsWalkingHash && parameter.type == AnimatorControllerParameterType.Bool)
                _hasWalkingParameter = true;

            if (parameter.nameHash == IsKnockDownHash && parameter.type == AnimatorControllerParameterType.Bool)
                _hasKnockDownParameter = true;

            if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasAttackParameter = true;

            if (parameter.nameHash == HurtHash && parameter.type == AnimatorControllerParameterType.Trigger)
                _hasHurtParameter = true;
        }
    }
}