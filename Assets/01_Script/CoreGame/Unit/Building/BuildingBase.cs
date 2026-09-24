using System;
using UnityEngine;

namespace _01_Script.CoreGame.Unit
{
    public abstract class BuildingBase : BaseEntity
    {
        [Header ("Building Blueprint")]
        public BuildingDataSO buildingDataSO => entityData as BuildingDataSO;
        public string BuildingName => buildingDataSO != null ? buildingDataSO.name : "Unknown";

        protected override void Awake()
        {
            base.Awake();
            if (buildingDataSO != null)
            {
                entityData = buildingDataSO;
            }
        }

        public override void TakeDamage(int amount)
        {
            if (IsDead) return;

            base.TakeDamage(amount);
            Debug.Log($"[{BuildingName}] Terkena {amount} damage. Sisa HP: {currentHealth}");
        }

        protected override void Die()
        {
            base.Die();

        }


    }
}