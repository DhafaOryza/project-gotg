using _01_Script.CoreGame.Unit.Building;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Script.CoreGame.Unit.Entity.Player
{
    public class PlayerCore : UnitCore
    {
        [Header ("Test Attacking")]
        [SerializeField] private float AtkRange = 2f;
        [SerializeField] private LayerMask BuildingLayer;
        protected override void Start()
        {
            Debug.Log("Player Telah Terbangun!");
            base.Start();
        }
        public override void Die()
        {
            base.Die();
            Debug.Log("Game Over! Karakter utama tewas.");
        }

        public void OnTestAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                TryAttackBuilding();
            }
        }

        private void TryAttackBuilding()
        {
            Collider2D[] hitCollider = Physics2D.OverlapCircleAll(transform.position, AtkRange, BuildingLayer);

            foreach (var col in hitCollider)
            {
                GranaryCore building = col.GetComponentInParent<GranaryCore>();
                if (building != null)
                {
                    building.TakeDamage(AttackDamage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, AtkRange);
        }
    }
}