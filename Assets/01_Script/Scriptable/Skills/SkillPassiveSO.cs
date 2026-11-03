using UnityEngine;

public enum PassiveTriggerType
{
    OnTakeDamage,   
    OnKillEnemy,    
    ConstantBuff    
}
[CreateAssetMenu(fileName = "NewPassiveSkill", menuName = "Data/Skill/Passive Skill Data")]
public class SkillPassiveSO : SkillDataSO
{
    [Header("Passive Configurations")]
    public PassiveTriggerType triggerType;
    public float triggerChance = 100f;
    public float duration = 5f;
    public int effectValue = 5;

    private void OnEnable()
    {
        executionType = SkillExecutionType.Passive;
    }

}