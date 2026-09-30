using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SkillSelectorCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Visual")]
    [SerializeField] private Image iconImage;

    private SkillDataSO skillData;
    private Canvas _rootCanvas;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Transform originalParent;

    public SkillDataSO SkillData => skillData;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(SkillDataSO data)
    {
        skillData = data;
        if (iconImage != null && data != null)
            iconImage.sprite = data.icon;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillData == null) return;

        originalParent = transform.parent;
        if (_rootCanvas != null)
        {
            transform.SetParent(_rootCanvas.transform, true);
        }

        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (skillData == null) return;
            _rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        // Jika tidak di-drop di tempat yang valid (parent tidak berubah saat proses Drop), kembalikan ke parent asal
        if (transform.parent == _rootCanvas.transform)
            ResetToOriginalParent();
    }

    public void SetNewParent(Transform newParent)
    {
        originalParent = newParent;
        transform.SetParent(_rootCanvas.transform, true);
    }
    public void ResetToOriginalParent() => transform.SetParent(originalParent, false);
}
