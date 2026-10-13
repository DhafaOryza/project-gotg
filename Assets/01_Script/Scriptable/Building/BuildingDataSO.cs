using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Data/Building/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    public int baseVitality = 10;
    public int baseDefend = 5;
    
    [Header("Stat Scaling")]
    public float hpPerVitality = 170f;

    
}