using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Script.CoreGame.Unit.Entity.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header ("Movement Setting")]
        [SerializeField] private float MoveSpeed = 10f;
        private Rigidbody2D rb;
        private Vector2 movementInput;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void OnMove(InputValue value)
        {
            float MoveX = value.Get<float>();
            movementInput = new Vector2(MoveX, 0f);
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = rb.position + movementInput * MoveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }
}
