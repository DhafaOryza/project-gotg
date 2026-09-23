using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Kartu skill di dalam "Skill Container" pada Canvas UI.
/// Cara pakai: drag kartu ini lalu drop ke target di world (enemy atau player sendiri).
/// - Drop ke enemy  -> menyerang (SkillTargetType.Enemy / Any)
/// - Drop ke player  -> buff pada diri sendiri (SkillTargetType.Self / Any)
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SkillCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Visual")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay; // opsional: radial fill 0-1, 0 = ready

    [Header("Drop Targeting")]
    [Tooltip("Set ke layer yang dipakai Enemy dan Player (harus punya Collider2D + komponen BaseEntity)")]
    [SerializeField] private LayerMask targetLayerMask = ~0;

    private SkillDataSO skillData;
    private int slotIndex;
    private PlayerBaseEntity owner;

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (owner == null || cooldownOverlay == null) return;

        float remaining = owner.GetSkillCooldownRemaining(slotIndex);
        var data = owner.GetSkillData(slotIndex);
        if (data == null || data.cooldown <= 0f)
        {
            cooldownOverlay.fillAmount = 0f;
            return;
        }

        cooldownOverlay.fillAmount = Mathf.Clamp01(remaining / data.cooldown);
    }

    /// <summary>Dipanggil oleh Skill Container saat mengisi/refresh slot kartu.</summary>
    public void Setup(PlayerBaseEntity owner, int slotIndex, SkillDataSO data)
    {
        this.owner = owner;
        this.slotIndex = slotIndex;
        this.skillData = data;

        if (iconImage != null) iconImage.sprite = data != null ? data.icon : null;
        gameObject.SetActive(data != null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillData == null || owner == null) return;
        if (!owner.CanAct || owner.GetSkillCooldownRemaining(slotIndex) > 0f) return;

        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;

        // Pindah ke root canvas supaya kartu render di atas UI lain selama drag
        if (rootCanvas != null) transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (skillData == null) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        BaseEntity target = ResolveWorldTarget(eventData);

        // Kartu selalu kembali ke slot asal; ini bukan "buang kartu", cuma cara input arah skill
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPos;

        if (skillData == null || owner == null) return;

        if (target != null)
            owner.TryUseSkill(slotIndex, target);
        else
            owner.NotifySkillDropMissed(slotIndex);
    }

    private BaseEntity ResolveWorldTarget(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;
        if (cam == null) return null;

        Vector2 worldPoint = cam.ScreenToWorldPoint(eventData.position);
        Collider2D hit = Physics2D.OverlapPoint(worldPoint, targetLayerMask);
        if (hit == null) return null;

        return hit.GetComponentInParent<BaseEntity>();
    }
}