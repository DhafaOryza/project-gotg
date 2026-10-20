using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    public List<SkillDataSO> choosenSkills = new List<SkillDataSO>();
    public int maxSkills = 5;

    public event Action OnChosenSkillsChanged;

    public int EquippedCount
    {
        get
        {
            int count = 0;
            foreach (var skill in choosenSkills)
                if (skill != null) count++;
            return count;
        }
    }

    public bool IsFull => FindEmptySlot() < 0;

    private void Awake()
    {
        EnsureSlotCount();
    }

    private void OnValidate()
    {
        if (maxSkills < 0) maxSkills = 0;
        EnsureSlotCount();
    }

    public SkillDataSO GetSkill(int index)
    {
        return index >= 0 && index < choosenSkills.Count ? choosenSkills[index] : null;
    }

    public bool AddSkill(SkillDataSO setSkill)
    {
        if (setSkill == null) return false;

        EnsureSlotCount();
        if (choosenSkills.Contains(setSkill)) return false;

        int empty = FindEmptySlot();
        if (empty < 0) return false;

        choosenSkills[empty] = setSkill;
        NotifyChanged();
        return true;
    }

    public bool EquipSkill(int slotIndex, SkillDataSO skill)
    {
        if (skill == null) return false;
        if (slotIndex < 0 || slotIndex >= maxSkills) return false;

        EnsureSlotCount();

        int existing = choosenSkills.IndexOf(skill);
        if (existing == slotIndex) return true; // sudah di slot itu

        SkillDataSO displaced = choosenSkills[slotIndex];
        choosenSkills[slotIndex] = skill;

        if (existing >= 0)
            choosenSkills[existing] = displaced;

        NotifyChanged();
        return true;
    }

    public bool RemoveSkill(SkillDataSO currentSkill)
    {
        if (currentSkill == null) return false;

        int index = choosenSkills.IndexOf(currentSkill);
        return RemoveSkillAt(index);
    }

    public bool RemoveSkillAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= choosenSkills.Count) return false;
        if (choosenSkills[slotIndex] == null) return false;

        choosenSkills[slotIndex] = null;
        NotifyChanged();
        return true;
    }

    public void ClearSkills()
    {
        bool changed = false;
        for (int i = 0; i < choosenSkills.Count; i++)
        {
            if (choosenSkills[i] == null) continue;
            choosenSkills[i] = null;
            changed = true;
        }

        if (changed) NotifyChanged();
    }

    private int FindEmptySlot()
    {
        int slots = Mathf.Min(choosenSkills.Count, maxSkills);
        for (int i = 0; i < slots; i++)
        {
            if (choosenSkills[i] == null) return i;
        }

        return choosenSkills.Count < maxSkills ? choosenSkills.Count : -1;
    }

    private void EnsureSlotCount()
    {
        while (choosenSkills.Count < maxSkills)
            choosenSkills.Add(null);

        if (choosenSkills.Count > maxSkills)
            choosenSkills.RemoveRange(maxSkills, choosenSkills.Count - maxSkills);
    }

    private void NotifyChanged()
    {
        OnChosenSkillsChanged?.Invoke();
    }
}