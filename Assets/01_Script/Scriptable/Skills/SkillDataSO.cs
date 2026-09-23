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

    [Tooltip("Jarak maksimum (dalam unit world) antara caster dan target supaya skill bisa diaktifkan. " +
             "Skill Self biasanya tidak perlu dibatasi, tapi tetap dicek kalau kamu isi > 0.")]
    public float range = 3f;

    [Header("Cooldown")]
    public float cooldown = 3f; // dikali entityData.skillCooldownMultiplier saat runtime

    [Header("Feedback")]
    public Color skillColor = Color.white; // "Warna/efek skill membedakan fungsi ability"

    /// <summary>
    /// Dipanggil PlayerBaseEntity.TryUseSkill sebelum Activate, untuk memvalidasi
    /// apakah target hasil drag & drop cocok dengan targetType skill ini.
    /// </summary>
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

    /// <summary>
    /// Cek apakah target masih berada dalam jangkauan skill ini. Dipanggil setelah IsValidTarget lolos.
    /// range <= 0 dianggap "tanpa batas jarak" (mis. skill self/buff yang tidak butuh jarak).
    /// </summary>
    public bool IsInRange(PlayerBaseEntity caster, BaseEntity target)
    {
        if (range <= 0f) return true;
        if (caster == null || target == null) return false;

        float distance = Vector2.Distance(caster.transform.position, target.transform.position);
        return distance <= range;
    }

    // Subclass ScriptableObject ini per skill (mis. SkillData_Trap, SkillData_Pukulan)
    // dan override Activate untuk efek nyata (target.TakeDamage(...) atau caster.ApplyBuff(...) dsb,
    // sesuaikan dengan API yang ada di BaseEntity kamu).
    public virtual void Activate(PlayerBaseEntity caster, BaseEntity target)
    {
        Debug.Log($"[Skill] {displayName} digunakan oleh {caster.name} ke target {(target != null ? target.name : "null")}");
    }
}