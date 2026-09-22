using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityData", menuName = "Data/Entity/Entity Data")]
public class EntityDataSO : ScriptableObject
{
    public int baseVitality = 10;

    [Header("Stat Scaling")]
    public float hpPerVitality = 170f;

    [Header("Skill Cooldown")]
    public float skillCooldownMultiplier = 1;

    [Header("Movement")]
    public float moveSpeed = 5f;
}