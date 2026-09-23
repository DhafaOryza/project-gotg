using UnityEngine;

public class SkillTestHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBaseEntity playerEntity;
    [SerializeField] private BaseEntity dummyTargetEnemy;

    [Header("Skill Settings")]
    [SerializeField] private int skillSlotToTest = 0; // Slot 0 = Skill Pertama (Hotkey 1)

    // Tombol di Inspector untuk memicu skill ke target musuh
    [ContextMenu("Test Cast Skill to Target Enemy")]
    public void TestCastSkillToTarget()
    {
        if (playerEntity == null)
        {
            Debug.LogWarning("[SkillTestHelper] PlayerBaseEntity belum di-assign!");
            return;
        }

        if (dummyTargetEnemy == null)
        {
            Debug.LogWarning("[SkillTestHelper] Dummy Target Enemy belum di-assign!");
            return;
        }

        Debug.Log($"[SkillTestHelper] Mencoba Cast Skill Slot {skillSlotToTest} ke {dummyTargetEnemy.name}...");
        
        // Memanggil TryUseSkill langsung dengan menyertakan target musuh
        bool success = playerEntity.TryUseSkill(skillSlotToTest, dummyTargetEnemy);

        if (success)
            Debug.Log("[SkillTestHelper] Skill BERHASIL di-cast!");
        else
            Debug.LogWarning("[SkillTestHelper] Skill GAGAL di-cast (Cek log error/cooldown/range).");
    }

    // Tombol di Inspector untuk memicu skill tanpa target (Ground/Aim Direction)
    [ContextMenu("Test Cast Skill (No Target / Ground)")]
    public void TestCastSkillNoTarget()
    {
        if (playerEntity == null)
        {
            Debug.LogWarning("[SkillTestHelper] PlayerBaseEntity belum di-assign!");
            return;
        }

        Debug.Log($"[SkillTestHelper] Mencoba Cast Skill Slot {skillSlotToTest} tanpa target...");
        
        bool success = playerEntity.TryUseSkill(skillSlotToTest, null);

        if (success)
            Debug.Log("[SkillTestHelper] Skill BERHASIL di-cast!");
        else
            Debug.LogWarning("[SkillTestHelper] Skill GAGAL di-cast.");
    }
}