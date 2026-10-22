using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RadialSlotBinding
{
    public string slotName;                  
    public KeyCode triggerKey;               
    public Transform socketParent;           
    public ConsumableDataSO boundData;

    [HideInInspector] public ConsumableItemSlotUI spawnedSlotUI;
}