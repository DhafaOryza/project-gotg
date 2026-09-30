using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SkillSelectorCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Visual")]
    [SerializeField] private Image iconImage;

    [Header("Juice / Feel Settings")]
    [SerializeField] private float followSpeed = 25f;
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    private SkillDataSO skillData;
    private Canvas _rootCanvas;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    private Transform homeParent;
    private Transform currentParent;
    
    private bool isDragging;
    private Vector2 targetPosition;
    private Vector2 lastPosition;

    public SkillDataSO SkillData => skillData;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(SkillDataSO data, Transform initialHome)
    {
        skillData = data;
        homeParent = initialHome;
        currentParent = initialHome;

        if (transform.parent != initialHome)
        {
            transform.SetParent(initialHome, false);
        }

        if (iconImage != null && data != null)
            iconImage.sprite = data.icon;
    }

    private void Update()
    {
        if (!isDragging) return;
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, targetPosition, Time.unscaledDeltaTime * followSpeed);

        // Efek miring (Tilt)
        Vector2 delta = (Vector2)_rectTransform.position - lastPosition;
        float targetTilt = Mathf.Clamp(-delta.x * 2f, -maxTiltAngle, maxTiltAngle);

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.unscaledDeltaTime * tiltSpeed);

        lastPosition = _rectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillData == null) return;

        isDragging = true;
        targetPosition = eventData.position;
        lastPosition = eventData.position;

        if (_rootCanvas != null)
        {
            transform.SetParent(_rootCanvas.transform, true);
        }

        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || skillData == null) return;
        targetPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        _canvasGroup.blocksRaycasts = true;

        transform.rotation = Quaternion.identity;

        // Jika dilepas tanpa ditangkap DropZone (masih menempel di Canvas)
        if (transform.parent == _rootCanvas.transform)
        {
            ResetToOriginalParent();
        }
    }

    public void SetNewParent(Transform newParent)
    {
        if (newParent == null) return;

        currentParent = newParent;
        transform.SetParent(newParent, false);
    }

    public void ResetToOriginalParent()
    {
        if (currentParent != null)
        {
            transform.SetParent(currentParent, false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;

        // Jika kartu sedang berada di SkillDeck, klik akan mengembalikannya ke Content/Home
        if (currentParent != homeParent && homeParent != null)
        {
            SetNewParent(homeParent);
        }
    }
}