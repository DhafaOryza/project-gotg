using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    private EnemyBaseEntity _enemyBase;
    private Rigidbody2D _rb;
    [SerializeField] private Transform _target;
    private bool _canMove = true;


    private void Awake()
    {
        _enemyBase = GetComponent<EnemyBaseEntity>();
        _rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if (!_canMove || _target == null)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        // Hitung arah horizontal (+1 kanan, -1 kiri)
        float directionX = Mathf.Sign(_target.position.x - transform.position.x);
        float speed = _enemyBase.entityData != null ? _enemyBase.entityData.moveSpeed : 2f;
        _rb.linearVelocity = new Vector2(directionX * speed, _rb.linearVelocity.y);

        // Flip seluruh objek (termasuk child/eye) via localScale
        if (directionX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    public void SetCanMove(bool state)
    {
        _canMove = state;
        if (!state)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }
}