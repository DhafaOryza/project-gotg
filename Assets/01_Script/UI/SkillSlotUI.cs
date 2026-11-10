using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IDropHandler
{
    [Header("References")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI keyBind;
    [SerializeField] private GameObject info;
    [SerializeField] private GameObject none;
    [SerializeField] private Button removeButton;

    private SkillDataSO skillData;
    private int slotIndex = -1;

    public int SlotIndex => slotIndex;

    private void Awake()
    {
        if (removeButton == null)
            removeButton = GetComponent<Button>();
        if (removeButton == null)
            removeButton = GetComponentInChildren<Button>();

        if (removeButton != null)
            removeButton.onClick.AddListener(RemoveSkillFromSlot);
    }

    private void OnDestroy()
    {
        if (removeButton != null)
            removeButton.onClick.RemoveListener(RemoveSkillFromSlot);
    }

    private void RemoveSkillFromSlot()
    {
        if (skillData == null || slotIndex < 0) return;

        SkillsManager skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null) return;

        skillsManager.RemoveSkillAt(slotIndex);
    }

    public void Initialize(string setKeyBind, SkillDataSO currentData = null, int index = -1)
    {
        skillData = currentData;
        slotIndex = index;
        

        if (keyBind != null)
        {
            bool hasKey = !string.IsNullOrEmpty(setKeyBind);
            keyBind.gameObject.SetActive(hasKey);

            if (keyBind.transform.parent != transform)
                keyBind.transform.parent.gameObject.SetActive(hasKey);

            if (hasKey)
                keyBind.text = setKeyBind;
        }
        
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
            Debug.LogWarning("SkillSlotUI.OnDrop: pointerDrag null.", this);
            return;
        }

        var card = dragged.GetComponent<SkillCard>();
        if (card == null || card.SkillData == null)
        {
            Debug.LogWarning($"SkillSlotUI.OnDrop: yang di-drop bukan SkillCard yang valid ({dragged.name}).", this);
            return;
        }

        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null)
        {
            Debug.LogError("SkillSlotUI: GameManager atau skillsManager tidak ditemukan.", this);
            return;
        }

        if (slotIndex < 0)
        {
            Debug.LogWarning("SkillSlotUI.OnDrop: slotIndex -1. Pastikan slot dibuat dengan Initialize(key, skill, i) dan i = indeks slot.", this);
            return;
        }

        bool success = skillsManager.EquipSkill(slotIndex, card.SkillData);
        if (!success)
        {
            Debug.LogWarning($"[SkillSlotUI] Tidak bisa memasang {card.SkillData.displayName} ke slot ini (Cek tipe Aktif vs Pasif)!");
        }
        // Pasang tepat di slot ini (Q = 0, W = 1, E = 2, dst).
        skillsManager.EquipSkill(slotIndex, card.SkillData);
    }
}
