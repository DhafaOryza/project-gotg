using _01_Script.CoreGame.Unit.Building;
using UnityEngine;

namespace _01_Script.CoreGame.Unit.Entity.Enemy
{
    public class EnemyCore : UnitCore
    {
        protected EnemyMovement movement;
        [SerializeField] private Transform targetTransform;

        protected override void Start() 
        {
            base.Start();
            movement = GetComponent<EnemyMovement>();

            if (targetTransform == null)
            {
                GranaryCore granary = FindFirstObjectByType<GranaryCore>();
                if (granary != null)
                {
                    targetTransform = granary.transform;
                }
            }

            if (targetTransform != null && movement != null)
            {
                movement.SetTarget(targetTransform);
            }
        }

        public override void Die()
        {
            if (movement != null)
            {
                movement.SetCanMove(false);
            }

            base.Die();
        }
    }
}