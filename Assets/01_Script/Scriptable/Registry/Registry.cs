using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRegistryData", menuName = "System/Registry/New Registry")]
public class Registry : ScriptableObject
{
    public PlayerDataSO defaultPlayerData;

    public List<SkillDataSO> defaultSkills = new List<SkillDataSO>();

    public List<ConsumableDataSO> defaultConsumables = new List<ConsumableDataSO>();
}