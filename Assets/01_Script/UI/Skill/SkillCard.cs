using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillCooldown;

    [Header("Drag")]
    [SerializeField, Range(0.1f, 1f)] private float dragSourceAlpha = 0.5f;

    public SkillDataSO SkillData { get; private set; }

    private Canvas rootCanvas;
    private RectTransform ghost;
    private CanvasGroup sourceGroup;

    public void Initialize(SkillDataSO currentSkill)
    {
        SkillData = currentSkill;

        if (currentSkill == null) return;
        if (skillIcon == null || skillName == null || skillCooldown == null) return;

        skillIcon.sprite = currentSkill.icon;
        skillName.text = currentSkill.displayName;
        skillCooldown.text = $"Cooldown : {currentSkill.cooldown}s";
    }

    #region Drag & Drop

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SkillData == null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        rootCanvas = canvas.rootCanvas;

        // Kartu asli tetap di tempatnya (supaya layout tidak rusak); yang bergerak adalah salinannya.
        var ghostObject = Instantiate(gameObject, rootCanvas.transform, false);
        ghost = ghostObject.GetComponent<RectTransform>();

        // Ukuran salinan disamakan dengan kartu asli.
        Vector2 size = ((RectTransform)transform).rect.size;
        ghost.anchorMin = ghost.anchorMax = new Vector2(0.5f, 0.5f);
        ghost.sizeDelta = size;
        ghost.localScale = Vector3.one;
        ghost.SetAsLastSibling();

        // blocksRaycasts = false penting: supaya slot di bawah kursor bisa menerima OnDrop.
        var ghostGroup = GetOrAddCanvasGroup(ghostObject);
        ghostGroup.blocksRaycasts = false;
        ghostGroup.alpha = 0.85f;

        sourceGroup = GetOrAddCanvasGroup(gameObject);
        sourceGroup.alpha = dragSourceAlpha;

        MoveGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // OnDrop di SlotUI dipanggil lebih dulu; di sini tinggal merapikan.
        CleanupDrag();
    }

    private void OnDisable()
    {
        CleanupDrag();
    }

    private void MoveGhost(PointerEventData eventData)
    {
        if (ghost == null || rootCanvas == null) return;

        var canvasRect = (RectTransform)rootCanvas.transform;
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, eventData.position, cam, out Vector2 localPoint))
        {
            ghost.localPosition = localPoint;
        }
    }

    private void CleanupDrag()
    {
        if (ghost != null)
        {
            Destroy(ghost.gameObject);
            ghost = null;
        }

        if (sourceGroup != null)
            sourceGroup.alpha = 1f;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (!target.TryGetComponent(out CanvasGroup group))
            group = target.AddComponent<CanvasGroup>();
        return group;
    }

    #endregion
}