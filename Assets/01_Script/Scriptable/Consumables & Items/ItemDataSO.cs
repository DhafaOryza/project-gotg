using UnityEngine;
public enum ItemCategory
{
    TRAP,
    DEFENSE,
    EXPLOSIVE,
    UTILITY,
    GRANARY
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Data/Item/Item Data")]
public abstract class ItemDataSO : ScriptableObject
{
    [Header("General Identity")]
    public string itemId;
    public string displayName;
    public Sprite icon;
    public ItemCategory category;

    [TextArea] public string description;
    public int maxStack = 5;

    public GameObject ItemPrefabs;

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
    protected abstract void OnItemSpawned(GameObject spawnedObject, PlayerBaseEntity user, Vector2 direction);
}