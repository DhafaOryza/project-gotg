using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _stackText;
    [SerializeField] private Button _selectButton;

    private ItemDataSO _data;
    private bool _canDrag;
    private Canvas _rootCanvas;
    private RectTransform _dragGhost;
    private CanvasGroup _sourceCanvasGroup;

    public event Action<ItemCardUI> OnSelected;

    public ItemDataSO Data => _data;

    public void SetDraggable(bool canDrag)
    {
        _canDrag = canDrag;
    }

    private void Awake()
    {
        if (_selectButton == null)
            _selectButton = GetComponent<Button>();

        if (_selectButton != null)
            _selectButton.onClick.AddListener(HandleSelectButton);
    }

    private void OnDestroy()
    {
        if (_selectButton != null)
            _selectButton.onClick.RemoveListener(HandleSelectButton);
    }

    /// <summary>
    /// Fills the card with the icon and name of one item.
    /// </summary>
    public void Initialize(ItemDataSO data)
    {
        _data = data;

        if (_icon != null)
        {
            _icon.sprite = data != null ? data.icon : null;
            _icon.enabled = _icon.sprite != null;
        }

        if (_nameText != null)
            _nameText.text = data != null ? data.displayName : string.Empty;
    }

    private void HandleSelectButton()
    {
        OnSelected?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_canDrag || _data == null) return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        _rootCanvas = canvas.rootCanvas;

        var ghostObject = Instantiate(gameObject, _rootCanvas.transform, false);
        _dragGhost = ghostObject.GetComponent<RectTransform>();
        _dragGhost.anchorMin = _dragGhost.anchorMax = new Vector2(0.5f, 0.5f);
        _dragGhost.sizeDelta = ((RectTransform)transform).rect.size;
        _dragGhost.localScale = Vector3.one;
        _dragGhost.SetAsLastSibling();

        var ghostCanvasGroup = GetOrAddCanvasGroup(ghostObject);
        ghostCanvasGroup.blocksRaycasts = false;
        ghostCanvasGroup.alpha = 0.85f;

        _sourceCanvasGroup = GetOrAddCanvasGroup(gameObject);
        _sourceCanvasGroup.alpha = 0.5f;
        MoveDragGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveDragGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDrag();
    }

    private void OnDisable()
    {
        CleanupDrag();
    }

    private void MoveDragGhost(PointerEventData eventData)
    {
        if (_dragGhost == null || _rootCanvas == null) return;

        var canvasRect = (RectTransform)_rootCanvas.transform;
        Camera camera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, eventData.position, camera, out Vector2 localPoint))
        {
            _dragGhost.localPosition = localPoint;
        }
    }

    private void CleanupDrag()
    {
        if (_dragGhost != null)
        {
            Destroy(_dragGhost.gameObject);
            _dragGhost = null;
        }

        if (_sourceCanvasGroup != null)
            _sourceCanvasGroup.alpha = 1f;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (!target.TryGetComponent(out CanvasGroup group))
            group = target.AddComponent<CanvasGroup>();

        return group;
    }
}