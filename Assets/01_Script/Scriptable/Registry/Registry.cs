using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRegistryData", menuName = "System/Registry/New Registry")]
public class Registry : ScriptableObject
{
    public EntityDataSO playerData;

    public List<SkillDataSO> SkillCard = new List<SkillDataSO>();
}