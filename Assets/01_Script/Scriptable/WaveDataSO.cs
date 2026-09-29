using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Data/Wave/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    public float timerPerWave = 180f;
    public float[] spawnIntervals = new[] { 3f, 5f, 8f };
    public List<WaveEnemy> waveEnemies = new List<WaveEnemy>();

    [System.Serializable]
    public class WaveEnemy
    {
        public enum WaveType
        {
            USUAL,
            BOSS
        }

        public WaveType waveType = WaveType.USUAL;
        public int spawnPerInterval = 3;
        public List<PoolIdSO> enemyIds = new List<PoolIdSO>();
    }
}