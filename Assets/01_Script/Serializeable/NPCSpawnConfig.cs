using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCSpawnConfig
{
    public PoolIdSO npcPoolId;
    [Tooltip("Berapa banyak NPC jenis ini yang ingin di-spawn")]
    public int spawnCount = 1;
}