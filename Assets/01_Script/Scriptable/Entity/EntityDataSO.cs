using UnityEngine;

public class EntityDataSO : ScriptableObject
{
    [Header ("Faction")]
    [SerializeField] protected FactionType _faction;
    public FactionType Faction => _faction;

    public int baseVitality = 10;

    [Header("Stat Scaling")]
    public float hpPerVitality = 170f;

    [Header("Skill Cooldown")]
    public float skillCooldownMultiplier = 1;

    [Header("Movement")]
    public float moveSpeed = 5f;
}