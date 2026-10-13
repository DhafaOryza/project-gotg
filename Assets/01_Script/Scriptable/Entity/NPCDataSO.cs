using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCData", menuName = "Data/Entity/NPC Data")]
public class NPCDataSO : EntityDataSO
{
    public string npcName;
    [TextArea(3, 5)]
    public string[] dialogueLines;
    public Sprite npcPortrait;

    [Header("Quest Offering")]
    [Tooltip("Quest yang akan ditawarkan NPC setelah percakapan selesai.")]
    public QuestDataSO assignedQuest;
}