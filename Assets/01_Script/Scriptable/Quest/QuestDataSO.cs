using UnityEngine;

public class QuestDataSO : ScriptableObject
{
    [Header("Quest Info")]
    public string questId;
    public string questTitle;
    [TextArea(2, 4)]
    public string questDescription;
    public QuestType questType;

    [Header("Rewards")]
    public int goldReward;
    public bool rewardConsumable;
    public ConsumableDataSO consumableReward;
}
