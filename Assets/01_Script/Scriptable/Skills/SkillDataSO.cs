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
    [Header("Targeting & Cooldown")]
    public string skillId;
    public string displayName;
    public int baseValue;
    public TypeRange typeRange;
    public SkillType skillType;

    [Header ("Skill Tier")]
    [Range (1, 4)] public int tier;


    [Header("UI & Visual")]
    public Sprite icon;
    [TextArea] public string description;
    
    [Header ("Progession Default")]
    public bool unlockByDefault = false;

    [Header("Targeting & Cooldown")]
    public SkillTargetType targetType = SkillTargetType.Enemy;
    public float cooldown = 3f;
    public Color skillColor = Color.white;

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