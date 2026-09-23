using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseEntity : BaseEntity
{
    [Header("Skill Loadout (3-5 skill)")]
    [SerializeField] private List<SkillDataSO> equippedSkills = new List<SkillDataSO>();

    [Header("Consumable Loadout")]
    [SerializeField] private List<ConsumableDataSO> equippedConsumables = new List<ConsumableDataSO>();
    [SerializeField] private int defaultConsumableQuantity = 2;

    protected override void Awake()
    {
        // GetOrCreate() sekarang menjamin GameSessionData sudah selesai EnsureDataLoaded()
        // (baik lewat instance yang sudah ada di scene maupun yang baru dibuat), jadi
        // baris-baris di bawah ini tidak lagi "balapan" dengan Start() milik GameSessionData.
        var session = GameSessionData.GetOrCreate();
        entityData = session.GetPlayerData();

        base.Awake(); // BaseEntity.Awake pakai entityData buat hitung maxHealth -> aman karena sudah terisi

        equippedSkills = session.GetSkillsData();
        equippedConsumables = session.GetConsumables();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Vector3 target = origin + (Vector3)AimDirection * 1.5f;
        Gizmos.DrawLine(origin,target);
        Gizmos.DrawWireSphere(target, 0.2f);
    }
}