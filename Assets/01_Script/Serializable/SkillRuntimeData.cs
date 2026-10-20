using UnityEngine;
/// <summary>
/// Menyimpan data runtime skill (cooldown, level, dll.) untuk tiap slot.
/// </summary>
[System.Serializable]
public class SkillRuntimeData
{
    public SkillDataSO skilldata;
    public float RemainingCooldown;

    public SkillRuntimeData(SkillDataSO data)
    {
        skilldata = data;
        RemainingCooldown = 0f;
    }

    public bool isOnCooldown => RemainingCooldown > 0;
    public void UpdateCooldown(float deltaTime)
    {
        if (RemainingCooldown > 0)
        {
            RemainingCooldown -= deltaTime;
            if (RemainingCooldown <= 0)
                RemainingCooldown = 0f;
        }
    }
    public void StartCooldown(float duration)
    {
        RemainingCooldown = duration;
    }
}