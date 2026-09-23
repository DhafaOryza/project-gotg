using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Entity/Player Data")]
public class PlayerDataSO : EntityDataSO
{
    private void OnEnable()
    {
       _faction = FactionType.ALLY;
    }
}