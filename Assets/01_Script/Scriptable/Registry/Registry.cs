using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRegistryData", menuName = "System/Registry/New Registry")]
public class Registry : ScriptableObject
{
    [Header("Enemies")]
    public List<EnemyDataSO> Enemies = new List<EnemyDataSO>();

    [Header("Skills")]
    public List<SkillDataSO> Skills = new List<SkillDataSO>();

    [Header("Consumables")]
    public List<ConsumableDataSO> Consumables = new List<ConsumableDataSO>();

    [Header("Waves")]
    public List<NightWaveDataSO> NightWaves = new List<NightWaveDataSO>();
}