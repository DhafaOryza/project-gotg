using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    private EnemyCore _enemyCore;
    private Rigidbody2D _rb;
    private Transform _target;
    private bool _canMove = true;

    private void Awake()
    {
        _enemyCore = GetComponent<EnemyCore>();
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

        // Ambil moveSpeed dari EntityDataSO milik BaseEntity
        float speed = _enemyCore.entityData != null ? _enemyCore.entityData.moveSpeed : 2f;
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