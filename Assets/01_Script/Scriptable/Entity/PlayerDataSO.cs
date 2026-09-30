using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Entity/Player Data")]
public class PlayerDataSO : EntityDataSO
{
    [Header ("Leveling Data")]
    public int level;
    public int maxExp = 100;
    public int currentExp;
    private void OnEnable()
    {
       _faction = FactionType.ALLY;
    }
}