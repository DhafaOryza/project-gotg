using UnityEngine;
using UnityEngine.InputSystem;
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

    private void FixedUpdate()
    {
        if (!_playerEntity.CanAct)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        Move();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
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