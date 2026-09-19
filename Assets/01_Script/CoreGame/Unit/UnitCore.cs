

using System;
using _01_Scripts.CoreGame.Unit;
using UnityEngine;

namespace _01_Script.CoreGame.Unit
{
    public abstract class UnitCore : MonoBehaviour
    {
        [Header ("Unit BluePrint")]
        [SerializeField] protected UnitDataSO unitDataSO;
        [Header ("Runtime State")]
        [SerializeField] protected float _currentHealth;
        protected bool isDead = false;


        //Pemanggilan Anti Nesting 
        public string UnitName => unitDataSO != null ? unitDataSO.unitName : "Unknown";
        // public FactionType Faction => unitDataSO != null ? unitDataSO.faction : FactionType.Neutral;
        public float MaxHP => unitDataSO != null ? unitDataSO.maxHP : 100f;
        public float MoveSpeed => unitDataSO != null ? unitDataSO.moveSpeed : 0f;

        public event Action OnDeath;

        protected virtual void Start()
        {
            if (unitDataSO != null) _currentHealth = MaxHP;
        }
        public virtual void TakeDamage(float amount)
        {
            if (isDead) return;


            _currentHealth -= amount;
            Debug.Log($"[{UnitName}] Terkena {amount} damage. Sisa HP: {_currentHealth}");

            if (_currentHealth >= 0)
            {
                _currentHealth = 0f;
             
            }
        }

        public virtual void Die()
        {
            isDead = true;
            OnDeath?.Invoke();
            Debug.Log($"[{UnitName}] Tewas!");

        }
    }
}

