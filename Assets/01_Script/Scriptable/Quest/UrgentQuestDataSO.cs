using UnityEngine;

[CreateAssetMenu(fileName = "NewUrgentQuest", menuName = "Data/Quest/Urgent Quest")]
public class UrgentQuestDataSO : QuestDataSO
{
    [Header("Urgent Config")]
    [Tooltip("Batas waktu dalam detik sebelum urgent quest gagal atau berakhir.")]
    public float timeLimitSeconds;
}