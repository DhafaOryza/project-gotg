using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // [Header("Interaction Check Settings")]
    // [SerializeField] private LayerMask npcLayerMask;

    private NPCBaseEntity nearestNPC;
    private PlayerBaseEntity playerEntity;

    private void Awake()
    {
        playerEntity = GetComponent<PlayerBaseEntity>();
    }

    private void Update()
    {
        FindNearestNPC();

        if (Input.GetKeyDown(KeyCode.E) && nearestNPC != null)
        {
            nearestNPC.Interact(playerEntity);
        }
    }

    private void FindNearestNPC()
    {
        // Cari seluruh NPCBaseEntity yang ada di dalam scene
        NPCBaseEntity[] allNPCs = FindObjectsByType<NPCBaseEntity>(FindObjectsSortMode.None);
        
        NPCBaseEntity closest = null;
        float minDistance = float.MaxValue;
        Vector2 playerPos = transform.position;

        foreach (var npc in allNPCs)
        {
            if (npc == null) continue;

            float distance = Vector2.Distance(playerPos, npc.transform.position);

            // Cek apakah player berada di dalam radius interaksi NPC tersebut
            if (distance <= npc.InteractionRadius)
            {
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = npc;
                }
            }
        }

        // Simpan referensi NPC terdekat yang valid di dalam radius
        if (nearestNPC != closest)
        {
            nearestNPC = closest;
            if (nearestNPC != null)
            {
                Debug.Log($"[PlayerInteraction] Mendekati NPC: {nearestNPC.name}. Tekan [E] untuk berinteraksi.");
            }
        }
    }
}