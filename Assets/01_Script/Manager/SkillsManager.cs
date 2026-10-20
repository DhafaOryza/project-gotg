using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    public List<SkillDataSO> choosenSkills = new List<SkillDataSO>();
    public int maxSkills = 5;

    public void AddSkill(SkillDataSO setSkill)
    {
        if (choosenSkills.Count > maxSkills) return;

        choosenSkills.Add(setSkill);
    }

    public void RemoveSkill(SkillDataSO currentSkill)
    {
        if (choosenSkills.Count == 0) return;

        if (choosenSkills.Contains(currentSkill))
        {
            choosenSkills.Remove(currentSkill);
        }
    }
}