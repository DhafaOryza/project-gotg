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
    [TextArea] public string description;
    public int baseValue;

    [Header("Targeting & Cooldown")]
    public TypeRange typeRange;
    public SkillType skillType;

    [Header("Skill Tier")]
    [Range(1, 4)] public int tier;


    [Header("UI & Visual")]
    public Sprite icon;

    [Header("Progession Default")]
    public bool locked = true;

    [Header("Targeting & Cooldown")]
    public SkillTargetType targetType = SkillTargetType.Enemy;
    public float cooldown = 3f;
    public Color skillColor = Color.white;

    [Header("Prefabs")]
    public GameObject skillPrefab;

    public bool Activate(PlayerBaseEntity user, float aimX)
    {
        if (skillPrefab == null)
        {
            Debug.LogWarning($"[{displayName}] skillPrefab belum diisi, skill tidak dijalankan.");
            return false;
        }

        Vector3 origin = user.transform.position;

        // Prefab bergerak ke arah transform.right, jadi cukup diputar 180 derajat untuk menghadap kiri.
        float yaw = aimX >= origin.x ? 0f : 180f;
        GameObject spawned = Instantiate(skillPrefab, origin, Quaternion.Euler(0f, yaw, 0f));

        if (spawned.TryGetComponent(out SkillProjectile projectile))
            projectile.Setup(user, baseValue);

        return true;
    }
}