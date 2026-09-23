using UnityEngine;
[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Data/Entity/Building Data")]
public class BuildingDataSO : EntityDataSO
{
    private void OnEnable()
    {
        _faction = FactionType.ALLY;
    }
}