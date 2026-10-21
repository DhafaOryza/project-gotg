using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class RadialSlotBinding
{
    public string slotName;
    public KeyCode triggerKey;         
    public Image slotIconImage;        
    public ConsumableDataSO boundData;
    [HideInInspector] public int currentQuantity;
}