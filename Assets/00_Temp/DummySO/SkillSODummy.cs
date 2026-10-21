using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillDummy", menuName = "Dummy/Skill/Skill Dummy Data")]
public class SkillSODummy : ScriptableObject
{
    [Header("General Info")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;

    [Header("Tier & Cost")]
    [Range(1, 4)]
    public int tier = 1;
    public int baseValue = 100; // Mewakili harga/cost skill

    [Header("Visual & State")]
    public Sprite icon;
    public bool locked = true;
}