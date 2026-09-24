// using System;
// using UnityEngine;

// namespace _01_Script.CoreGame.Unit
// {
//     public abstract class BuildingCore : MonoBehaviour
//     {
//         [Header ("Building Blueprint")]
//         [SerializeField] protected BuildingDataSO buildingDataSO;
//         [Header ("Runtime")]
//         [SerializeField] protected float _currentHealth;
//         protected bool isDestroyed = false;
//         protected SpriteRenderer _sr;

//         public string BuildingName => buildingDataSO != null ? buildingDataSO.BuildingName : "Unknown";
//         public float MaxHP => buildingDataSO != null ? buildingDataSO.MaxHP : 100f;

//         public event Action OnDestroy;

//         protected void Awake()
//         {
//             _sr = GetComponentInChildren<SpriteRenderer>();
//         }

//         protected virtual void Start()
//         {
//             if (buildingDataSO != null) _currentHealth = MaxHP;
//         }

//         public virtual void TakeDamage(float amount)
//         {
//             if (isDestroyed) return;

//             _currentHealth -= amount;
//             Debug.Log($"[{BuildingName}] Terkena {amount} damage. Sisa HP: {_currentHealth}");

//             if (_currentHealth <= 0)
//             {
//                 _currentHealth = 0;
//                 Die();
//             }
//         }

//         protected virtual void Die()
//         {
//             isDestroyed = true;
//             OnDestroy?.Invoke();

//             if (_sr != null)
//             {
//                 _sr.enabled = false;
//             }
//         }

//     }
// }