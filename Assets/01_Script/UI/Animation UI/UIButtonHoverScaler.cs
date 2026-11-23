using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder;

public class UIButtonHoverScaler : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    ISelectHandler, IDeselectHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoveredScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private Vector3 pressedScale = new Vector3(0.92f, 0.92f, 1f);
    [SerializeField] private float transitionSpeed = 16f;

    private Coroutine scaleCoroutine;
    private bool isHovered = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StartScaleAnimation(hoveredScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StartScaleAnimation(normalScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartScaleAnimation(isHovered ? hoveredScale : normalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartScaleAnimation(pressedScale);
    }

    public void OnSelect(BaseEventData eventData)
    {
        isHovered = true;
        StartScaleAnimation(hoveredScale);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isHovered = false;
        StartScaleAnimation(normalScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.002f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, transitionSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}