using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyState currentState = EnemyState.IDLE;
    private EnemyBaseEntity _entity;
    private EnemyTargeter _targeter;
    private EnemyMovement _movement;
    private float _executeCooldownTimer;
    private float _executeDurationTimer;
    private bool _hasPerformedAction = false;

    private void Awake()
    {
        _entity = GetComponent<EnemyBaseEntity>();
        _movement = GetComponent<EnemyMovement>();
        _targeter = GetComponent<EnemyTargeter>();
    }

    private void Update()
    {
        if (_entity.enemyData != null && _entity.currentHealth <= 0 && currentState != EnemyState.DIE)
        {
            ChangeState(EnemyState.DIE);
            return;
        }

        switch (currentState)
        {
            case EnemyState.IDLE:
                HandleIdleState();
                break;
            case EnemyState.WALK:
                HandleWalkState();
                break;
            case EnemyState.EXECUTE:
                HandleExecuteState();
                break;
            case EnemyState.DIE:
                HandleDieState();
                break;
        }
    }

    private void HandleIdleState()
    {
        _movement.SetCanMove(false);
        if (_executeCooldownTimer > 0f)
        {
            _executeCooldownTimer -= Time.deltaTime;
        }

        if (_executeCooldownTimer <= 0f && _targeter.Hastarget)
        {
            if (_targeter.isInRange)
                ChangeState(EnemyState.EXECUTE);
            else
                ChangeState(EnemyState.WALK);
        }
    }
    private void HandleWalkState()
    {
        if (!_targeter.Hastarget)
        {
            ChangeState(EnemyState.IDLE);
            return;
        }
            
        if (_targeter.isInRange)
        {
            ChangeState(EnemyState.EXECUTE);
            return;
        }
            
        _movement.SetTarget(_targeter.currentTarget.transform);
        _movement.SetCanMove(true);
    }
    private void HandleExecuteState()
    {
        _movement.SetCanMove(false);
        if (!_hasPerformedAction)
        {
            if (_targeter.currentTarget != null)
            {
                _entity.PerformAction(_targeter.currentTarget);
                Debug.Log("Musuh Menyerang !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }

            _hasPerformedAction = true;
            _executeDurationTimer = 0.5f;
        }
        
        if (_executeDurationTimer > 0f)
        {
            _executeDurationTimer -= Time.deltaTime;
            return;
        }

        if (_entity.enemyData != null)
            _executeCooldownTimer = _entity.enemyData.executeCooldown;

        _hasPerformedAction = false;
        ChangeState(EnemyState.IDLE);
    }
    private void HandleDieState()
    {
        _movement.SetCanMove(false);
   
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    public void ResetFSM()
    {
        currentState = EnemyState.IDLE;
        _executeCooldownTimer = 0;
        _movement.SetCanMove(true);
    }
}
