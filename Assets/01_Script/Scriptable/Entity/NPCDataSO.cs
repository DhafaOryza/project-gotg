using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCData", menuName = "Data/Entity/NPC Data")]
public class NPCDataSO : EntityDataSO
{
    [SerializeField] private string _npcName;
    [SerializeField] [TextArea(3, 5)] private string[] _dialogueLines;
    [SerializeField] private Sprite _npcPortrait;

    [Header("Quest Offering")]
    [SerializeField] private QuestDataSO _assignedQuest;

    public string NpcName => _npcName;
    public string[] DialogueLines => _dialogueLines;
    public Sprite NpcPortrait => _npcPortrait;
    public QuestDataSO AssignedQuest => _assignedQuest;
}