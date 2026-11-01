using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Data/Building/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    public string nameBuilding;
    public int baseVitality = 10;
    public int baseDefend = 5;

    [Header("Stat Scaling")]
    public float hpPerVitality = 170f;

    [Header("Additional Stats")]
    public List<AdditionalStatsSO> additionalStats = new List<AdditionalStatsSO>();

    public float TotalVitality =>
        baseVitality + AdditionalStatsSO.CalculateTotal(StatsType.Vitality, additionalStats);

    public float MaxHealth => Mathf.Max(0f, TotalVitality * hpPerVitality);

    // Defense is damage reduction percent, clamped to 0–100%.
    public float TotalDefense =>
        Mathf.Clamp(baseDefend + AdditionalStatsSO.CalculateTotal(StatsType.Defense, additionalStats), 0f, 100f);

    public float GetDamageAfterDefense(float incomingDamage)
    {
        return Mathf.Max(0f, incomingDamage * (1f - TotalDefense / 100f));
    }
}