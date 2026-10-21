using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCData", menuName = "Data/Entity/NPC Data")]
public class NPCDataSO : EntityDataSO
{
    [SerializeField] private string _npcId;
    [SerializeField] private string _npcName;
    [SerializeField] private NpcRole _npcRole = NpcRole.Villager;
    [SerializeField] private bool _isInteractable = true;
    [SerializeField] [TextArea(3, 5)] private string[] _dialogueLines;
    [SerializeField] private Sprite _npcPortrait;

    [Header("Quest Offering")]
    [SerializeField] private QuestDataSO _assignedQuest;

    public string NpcId => _npcId;
    public string NpcName => _npcName;
    public NpcRole NpcRole => _npcRole;
    public bool IsInteractable => _isInteractable;
    public string[] DialogueLines => _dialogueLines;
    public Sprite NpcPortrait => _npcPortrait;
    public QuestDataSO AssignedQuest => _assignedQuest;
}