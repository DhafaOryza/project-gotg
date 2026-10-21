using UnityEngine;

public enum ConsumablesType
{
    RECOVERY,
    BUFF,
    SPECIAL,
    EMERGENCY,
}

[CreateAssetMenu(fileName = "NewConsumableData", menuName = "Data/Consumable/Consumable Data")]
public class ConsumableDataSO : ScriptableObject
{
     [Header("General Info")]
    public string consumableId;
    public string displayName;
    public Sprite icon;
    public ConsumablesType consumableType;

    [TextArea] public string description;
    public int maxStack = 10;
    public int baseValue;

    // Subclass untuk efek nyata (heal darurat, buff sementara, dll)
    public virtual bool Use(PlayerBaseEntity user)
    {
        switch (consumableType)
        {
            case ConsumablesType.RECOVERY:
                if (user.currentHealth >= user.maxHealth)
                {
                    Debug.Log($"[{displayName}] HP Player sudah penuh!");
                    return false;
                }
                user.Heal(baseValue);
                return true;

            default:
                Debug.Log($"[Consumable] {displayName} digunakan oleh {user.name}");
                return true;
        }
    }
}