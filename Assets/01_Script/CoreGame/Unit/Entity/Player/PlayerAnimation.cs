using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerBaseEntity _playerBaseEntity;
    [SerializeField] private float movementThreshold = 0.1f;

    private Rigidbody _playerRigidbody;

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
    }

    private void Update()
    {
        if (_animator == null || _playerBaseEntity == null || _playerRigidbody == null)
            return;

        bool isWalking = _playerBaseEntity.CanAct &&
            Mathf.Abs(_playerRigidbody.linearVelocity.x) > movementThreshold;

        if (HasParameter(IsWalkingHash, AnimatorControllerParameterType.Bool))
            _animator.SetBool(IsWalkingHash, isWalking);
    }

    public bool PlayAttack()
    {
        if (_animator == null || !HasParameter(AttackHash, AnimatorControllerParameterType.Trigger))
            return false;

        _animator.ResetTrigger(AttackHash);
        _animator.SetTrigger(AttackHash);
        return true;
    }

    public void OnAttackAnimationEvent()
    {
        if (_playerBaseEntity == null)
            _playerBaseEntity = GetComponentInParent<PlayerBaseEntity>();

        _playerBaseEntity?.ExecutePendingSkillFromAnimationEvent();
    }

    private bool HasParameter(int parameterHash, AnimatorControllerParameterType parameterType)
    {
        if (_animator == null) return false;

        foreach (var parameter in _animator.parameters)
        {
            if (parameter.nameHash == parameterHash && parameter.type == parameterType)
                return true;
        }

        return false;
    }
}