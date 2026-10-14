using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableData", menuName = "Data/Consumable/Consumable Data")]
public class ConsumableDataSO : ScriptableObject
{
    public string consumableId;
    public string displayName;
    public Sprite icon;

    [TextArea] public string description;
    public int maxStack = 5;

    // Subclass untuk efek nyata (heal darurat, buff sementara, dll)
    public virtual void Use(PlayerBaseEntity user)
    {
        Debug.Log($"[Consumable] {displayName} digunakan oleh {user.name}");
    }
}