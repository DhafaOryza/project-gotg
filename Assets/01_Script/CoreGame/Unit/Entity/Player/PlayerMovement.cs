using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Script.CoreGame.Unit.Entity.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header ("Movement Setting")]
        private PlayerCore _playerCore;
        private Rigidbody2D _rb;
        private Vector2 movementInput;
        void Start()
        {
            _playerCore = GetComponent<PlayerCore>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            float MoveX = ctx.ReadValue<float>();
            movementInput = new Vector2(MoveX, 0f);
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = _rb.position + movementInput * _playerCore.MoveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);
        }
    }
}
