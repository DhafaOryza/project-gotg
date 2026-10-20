using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IDropHandler
{
    [Header("References")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI keyBind;
    [SerializeField] private GameObject info;
    [SerializeField] private GameObject none;

    private SkillDataSO skillData;
    private int slotIndex = -1;

    public int SlotIndex => slotIndex;

    public void Initialize(string setKeyBind, SkillDataSO currentData = null, int index = -1)
    {
        skillData = currentData;
        slotIndex = index;
        keyBind.text = setKeyBind;

        if (currentData != null)
        {
            info.SetActive(true);
            skillIcon.sprite = skillData.icon;
            skillName.text = skillData.displayName;

            none.SetActive(false);
        }
        else
        {
            info.SetActive(false);
            none.SetActive(true);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Log sementara untuk melacak di mana drop berhenti. Hapus kalau sudah beres.
        var dragged = eventData.pointerDrag;
        if (dragged == null)
        {
            Debug.LogWarning("SlotUI.OnDrop: pointerDrag null.", this);
            return;
        }

        var card = dragged.GetComponent<SkillCard>();
        if (card == null || card.SkillData == null)
        {
            Debug.LogWarning($"SlotUI.OnDrop: yang di-drop bukan SkillCard yang valid ({dragged.name}).", this);
            return;
        }

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null)
        {
            Debug.LogError("SlotUI: GameManager atau skillsManager tidak ditemukan.", this);
            return;
        }

        if (slotIndex < 0)
        {
            Debug.LogWarning("SlotUI.OnDrop: slotIndex -1. Pastikan slot dibuat dengan Initialize(key, skill, i) dan i = indeks slot.", this);
            return;
        }

        // Pasang tepat di slot ini (Q = 0, W = 1, E = 2, dst).
        skillsManager.EquipSkill(slotIndex, card.SkillData);
    }
}