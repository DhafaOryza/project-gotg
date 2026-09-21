using UnityEngine;

namespace _01_Script.CoreGame.Unit.Entity.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        private EnemyCore _enemyCore;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Transform _target;
        private bool _canMove = true;

        private void Awake()
        {
            _enemyCore = GetComponent<EnemyCore>();
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (!_canMove || _target == null)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float directionX = Mathf.Sign(_target.position.x - transform.position.x);
            _rb.linearVelocity = new Vector2(directionX * _enemyCore.MoveSpeed, _rb.linearVelocity.y);

            if (directionX > 0)
            {
                _sr.transform.localScale = new Vector3(1,1,1);
            }
            else if (directionX < 0)
            {
                _sr.transform.localScale = new Vector3(-1,1,1);
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
}