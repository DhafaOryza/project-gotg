using UnityEngine;

public enum ItemCategory
{
    TRAP,
    DEFENSE,
    EXPLOSIVE,
    UTILITY,
    GRANARY
}

public enum ItemType
{
    RECOVERY,
    BUFF,
    SPECIAL,
    EMERGENCY,
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Data/Item/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("General Info")]
    public string itemId;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 10;

    [Header("Deployable Item")]
    public ItemCategory category;
    public GameObject ItemPrefabs;

    [Header("Shop Item")]
    public ItemType itemType;
    public int price = 100;
    public int stack;
    public int baseValue;
    [Header("VFX")]
    [Tooltip("Di-spawn di atas user saat item RECOVERY dipakai (auto-hilang setelah 2.5 detik).")]
    public GameObject healVfxPrefab;

    public virtual bool Use(PlayerBaseEntity user)
    {
        if (user == null) return false;

        switch (itemType)
        {
            case ItemType.RECOVERY:
                if (user.currentHealth >= user.maxHealth)
                {
                    Debug.Log($"[{displayName}] HP Player sudah penuh!");
                    return false;
                }

                user.Heal(baseValue);

                if (healVfxPrefab != null)
                {
                    GameObject fx = Instantiate(healVfxPrefab, user.transform.position + Vector3.up * 0.9f, Quaternion.identity);
                    fx.transform.SetParent(user.transform, true);
                    Destroy(fx, 2.5f);
                }
                return true;

            default:
                Debug.Log($"[Item] {displayName} digunakan oleh {user.name}");
                return true;
        }
    }

    public virtual bool Deploy(PlayerBaseEntity user, Vector3 spawnPosition, Vector2 direction = default)
    {
        if (ItemPrefabs == null)
        {
            Debug.LogWarning($"[{displayName}] Prefab belum diisi!");
            return false;
        }

        GameObject spawned = Instantiate(ItemPrefabs, spawnPosition, Quaternion.identity);
        OnItemSpawned(spawned, user, direction);
        return true;
    }

    protected virtual void OnItemSpawned(GameObject spawnedObject, PlayerBaseEntity user, Vector2 direction)
    {
    }
}
