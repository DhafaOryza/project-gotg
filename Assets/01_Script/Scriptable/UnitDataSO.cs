using UnityEngine;

namespace _01_Scripts.CoreGame.Unit
{
    [CreateAssetMenu(fileName = "New Unit Data", menuName = "Granary Defense/Unit Data")]
    public class UnitDataSO : ScriptableObject
    {
        [Header("Identity & Classification")]
        [Tooltip("Sesuai konvensi: CHR_Player_Village_Guardian / ENM_Goblin_Melee_Normal")]
        public string unitID;
        public string unitName;
        // public FactionType faction;
        // public ThreatTier threatTier;

        [Header("Base Stats")]
        public float maxHP = 100f;
        public float moveSpeed = 4f;
        public float attackDamage = 10f;
        public float attackCooldown = 1.5f;
        public float knockbackResistance = 0f;
    }
}