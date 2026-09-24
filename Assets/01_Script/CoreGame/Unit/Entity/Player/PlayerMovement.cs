using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerBaseEntity _playerEntity;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerEntity = GetComponent<PlayerBaseEntity>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        _moveInput = new Vector2(moveX, moveY);
    }
    private void FixedUpdate()
    {
        if (!_playerEntity.CanAct)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }
        
        Move();
    }

    private void Move()
    {
        float speed = _playerEntity.entityData != null ? _playerEntity.entityData.moveSpeed : 5f;
        _rb.linearVelocity = new Vector2(_moveInput.x * speed , _rb.linearVelocity.y);

        if (_moveInput.x > 0)
            transform.localScale = new Vector3(1,1,1);
        else if (_moveInput.x < 0)
            transform.localScale = new Vector3(-1,1,1);
    }
}