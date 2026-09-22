// using _01_Script.Enum;
// using UnityEngine;

// namespace _01_Scripts.CoreGame.Unit
// {
//     [CreateAssetMenu(fileName = "New Unit Data", menuName = "Granary Defense/Unit Data")]
//     public class UnitDataSO : ScriptableObject
//     {
//         [Header("Identity & Classification")]
//         [Tooltip("Sesuai konvensi: CHR_Player_Village_Guardian / ENM_Goblin_Melee_Normal")]
//         [SerializeField] private string _unitID;
//         [SerializeField] private string _unitName;
//         [SerializeField] private FactionType _faction = FactionType.NEUTRAL;
//         [SerializeField] private ThreatTier _threatTier;

//         [Header("Base Stats")]
//         [SerializeField] private float _maxHP = 100f;
//         [SerializeField] private float _moveSpeed = 4f;
//         [SerializeField] private float _attackDamage = 10f;
//         [SerializeField] private float _attackCooldown = 1.5f;
//         [SerializeField] private float _knockbackResistance = 0f;

//         public string UnitID => _unitID;
//         public string UnitName => _unitName;
//         public FactionType Faction => _faction;
//         public ThreatTier ThreatTier => _threatTier;

//         public float MaxHP => _maxHP;
//         public float MoveSpeed => _moveSpeed;
//         public float AttacDamage => _attackDamage;
//         public float AttackCooldown => _attackCooldown;
//         public float KnockbackResistance => _knockbackResistance;
//     }
// }