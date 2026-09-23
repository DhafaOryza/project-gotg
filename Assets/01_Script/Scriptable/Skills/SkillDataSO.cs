using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Data/Skill/raw Data")]
public class SkillDataSO : ScriptableObject
{
    public string skillId;
    public string displayName;
    public Sprite icon;

    [TextArea] public string description;

    [Header("Cooldown")]
    public float cooldown = 3f; // dikali entityData.skillCooldownMultiplier saat runtime

    [Header("Feedback")]
    public Color skillColor = Color.white; // "Warna/efek skill membedakan fungsi ability"

    // Subclass ScriptableObject ini per skill (mis. SkillData_Trap, SkillData_Pukulan)
    // dan override Activate untuk efek nyata.
    public virtual void Activate(PlayerBaseEntity caster)
    {
        Debug.Log($"[Skill] {displayName} digunakan oleh {caster.name}");
    }
}