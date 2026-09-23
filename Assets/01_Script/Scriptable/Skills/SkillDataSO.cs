using UnityEngine;

// Menentukan siapa yang boleh menerima skill ini saat di-drop
public enum SkillTargetType
{
    Enemy,  // hanya valid di-drop ke musuh -> efek serangan
    Self,   // hanya valid di-drop ke diri sendiri -> efek buff
    Any     // valid ke keduanya, efek ditentukan di Activate() berdasarkan target
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Data/Skill/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    public string skillId;
    public string displayName;
    public Sprite icon;

    [TextArea] public string description;

    [Header("Targeting")]
    public SkillTargetType targetType = SkillTargetType.Enemy;

    [Header("Cooldown")]
    public float cooldown = 3f; // dikali entityData.skillCooldownMultiplier saat runtime

    [Header("Feedback")]
    public Color skillColor = Color.white; // "Warna/efek skill membedakan fungsi ability"

    public bool IsValidTarget(PlayerBaseEntity caster, BaseEntity target)
    {
        if (target == null) return false;

        bool isSelf = target == (BaseEntity)caster;

        switch (targetType)
        {
            case SkillTargetType.Enemy: return !isSelf;
            case SkillTargetType.Self: return isSelf;
            default: return true; // Any
        }
    }

    public virtual void Activate(PlayerBaseEntity caster, BaseEntity target)
    {
        Debug.Log($"[Skill] {displayName} digunakan oleh {caster.name} ke target {(target != null ? target.name : "null")}");
    }
}