

// using System;
// using _01_Scripts.CoreGame.Unit;
// using UnityEngine;

// namespace _01_Script.CoreGame.Unit
// {
//     public abstract class UnitCore : MonoBehaviour
//     {
//         [Header ("Unit BluePrint")]
//         [SerializeField] protected UnitDataSO unitDataSO;
//         [Header ("Runtime")]
//         [SerializeField] protected float _currentHealth;
//         protected bool isDead = false;
//         protected Collider2D _col;
//         protected Rigidbody2D _rb;


//         //Pemanggilan Anti Nesting 
//         public string UnitName => unitDataSO != null ? unitDataSO.UnitName : "Unknown";
//         // public FactionType Faction => unitDataSO != null ? unitDataSO.faction : FactionType.Neutral;
//         public float MaxHP => unitDataSO != null ? unitDataSO.MaxHP : 100f;
//         public float MoveSpeed => unitDataSO != null ? unitDataSO.MoveSpeed : 0f;
//         public float AttackDamage => unitDataSO != null ? unitDataSO.AttacDamage : 10f;

//         public event Action OnDeath;

//         protected void Awake()
//         {
//             _col = GetComponentInChildren<Collider2D>();
//             _rb = GetComponent<Rigidbody2D>();
//         }
//         protected virtual void Start()
//         {
//             if (unitDataSO != null) _currentHealth = MaxHP;
//         }

//         public virtual void TakeDamage(float amount)
//         {
//             if (isDead) return;


//             _currentHealth -= amount;
//             Debug.Log($"[{UnitName}] Terkena {amount} damage. Sisa HP: {_currentHealth}");

//             if (_currentHealth <= 0)
//             {
//                 _currentHealth = 0f;
//                 Die();
             
//             }
//         }

//         public virtual void Die()
//         {
//             isDead = true;
//             OnDeath?.Invoke();
//             Debug.Log($"[{UnitName}] Tewas!");

//             if (_col != null)
//             {
//                 _col.enabled = false;
//             }

//             if (_rb != null)
//             {
//                 _rb.constraints = RigidbodyConstraints2D.FreezePositionX;
//             }

//         }
//     }
// }

