using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to the Content transform of a scroll view or any container.
/// Animates all direct children when the container becomes active,
/// and adds a press-shrink effect to any Selectable among them.
/// </summary>
public class ContainerAnimator : MonoBehaviour
{
    public enum AnimationType
    {
        PopUp,
        Slide,
    }

    [Header("Animation Type")]
    [SerializeField] private AnimationType _animationType = AnimationType.PopUp;

    [Header("Shared Settings")]
    [SerializeField, Min(0f)] private float _duration = 0.35f;
    [SerializeField, Min(0f)] private float _staggerDelay = 0.04f;

    [Header("PopUp Settings")]
    [SerializeField] private AnimationCurve _popUpCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.6f, 1.1f),
        new Keyframe(0.8f, 0.95f),
        new Keyframe(1f, 1f));

    [Header("Slide Settings")]
    [SerializeField] private SlideDirection _slideFrom = SlideDirection.Right;
    [SerializeField, Min(0f)] private float _slideStartOffset = 300f;
    [SerializeField] private AnimationCurve _slideCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f));

    [Header("Press Shrink")]
    [SerializeField] private bool _enablePressShrink = true;
    [SerializeField, Range(0.5f, 1f)] private float _pressScale = 0.9f;
    [SerializeField, Min(0f)] private float _pressDuration = 0.08f;

    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down,
    }

    private readonly List<RectTransform> _children = new List<RectTransform>();
    private readonly List<PressShrinkHandler> _handlers = new List<PressShrinkHandler>();
    private Coroutine _animationCoroutine;

    private void OnEnable()
    {
        // Wait one frame so newly instantiated children are ready.
        StartCoroutine(PlayAfterFrame());
    }

    private void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    private void OnTransformChildrenChanged()
    {
        if (isActiveAndEnabled)
            StartCoroutine(PlayAfterFrame());
    }

    private IEnumerator PlayAfterFrame()
    {
        yield return null;
        CollectChildren();
        PlayAnimation();
    }

    private void CollectChildren()
    {
        // Remove old press handlers before collecting new children.
        foreach (var handler in _handlers)
        {
            if (handler != null)
                Destroy(handler);
        }
        _handlers.Clear();
        _children.Clear();

        foreach (RectTransform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;

            _children.Add(child);

            if (!_enablePressShrink) continue;

            // Add a press handler to the child or to any Selectable inside it.
            var selectable = child.GetComponentInChildren<Selectable>(false);
            if (selectable == null) continue;

            var target = selectable.GetComponent<RectTransform>();
            if (target == null) continue;

            var handler = selectable.gameObject.AddComponent<PressShrinkHandler>();
            handler.Setup(target, _pressScale, _pressDuration);
            _handlers.Add(handler);
        }
    }

    private void PlayAnimation()
    {
        if (_children.Count == 0) return;

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = _animationType == AnimationType.PopUp
            ? StartCoroutine(PlayPopUp())
            : StartCoroutine(PlaySlide());
    }

    #region PopUp

    private IEnumerator PlayPopUp()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i] != null)
                _children[i].localScale = Vector3.zero;
        }

        for (int i = 0; i < _children.Count; i++)
        {
            int index = i;
            StartCoroutine(AnimatePopUpChild(_children[index], i * _staggerDelay));
        }

        yield return null;
        _animationCoroutine = null;
    }

    private IEnumerator AnimatePopUpChild(RectTransform child, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            if (child == null) yield break;

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            float scale = _popUpCurve.Evaluate(t);
            child.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        if (child != null)
            child.localScale = Vector3.one;
    }

    #endregion

    #region Slide

    private IEnumerator PlaySlide()
    {
        Vector2 offset = GetSlideOffset();

        // Store original anchored positions and push children to start offset.
        var origins = new Vector2[_children.Count];
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i] == null) continue;

            origins[i] = _children[i].anchoredPosition;
            _children[i].anchoredPosition = origins[i] + offset;
            _children[i].localScale = Vector3.one;
        }

        for (int i = 0; i < _children.Count; i++)
        {
            int index = i;
            StartCoroutine(AnimateSlideChild(_children[index], origins[index], offset, index * _staggerDelay));
        }

        yield return null;
        _animationCoroutine = null;
    }

    private IEnumerator AnimateSlideChild(RectTransform child, Vector2 targetPosition, Vector2 startOffset, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Vector2 from = targetPosition + startOffset;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            if (child == null) yield break;

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            child.anchoredPosition = Vector2.LerpUnclamped(from, targetPosition, _slideCurve.Evaluate(t));
            yield return null;
        }

        if (child != null)
            child.anchoredPosition = targetPosition;
    }

    private Vector2 GetSlideOffset()
    {
        return _slideFrom switch
        {
            SlideDirection.Left => new Vector2(-_slideStartOffset, 0f),
            SlideDirection.Right => new Vector2(_slideStartOffset, 0f),
            SlideDirection.Up => new Vector2(0f, _slideStartOffset),
            SlideDirection.Down => new Vector2(0f, -_slideStartOffset),
            _ => new Vector2(_slideStartOffset, 0f),
        };
    }

    #endregion

    /// <summary>
    /// Call this after adding children dynamically (e.g. after GenerateSkillCard finishes).
    /// </summary>
    public void Refresh()
    {
        if (isActiveAndEnabled)
            StartCoroutine(PlayAfterFrame());
    }
}

/// <summary>
/// Added at runtime to a child Selectable. Shrinks it on press and restores on release.
/// Destroyed when the container rebuilds its children list.
/// </summary>
public class PressShrinkHandler : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    private RectTransform _target;
    private float _pressScale;
    private float _duration;
    private Vector3 _originalScale;
    private Coroutine _coroutine;
    private bool _isSetup;

    public void Setup(RectTransform target, float pressScale, float duration)
    {
        _target = target;
        _pressScale = pressScale;
        _duration = duration;
        _originalScale = target.localScale;
        _isSetup = true;
    }

    public void OnPointerDown(PointerEventData eventData) => AnimateTo(_pressScale);
    public void OnPointerUp(PointerEventData eventData) => AnimateTo(1f);
    public void OnSelect(BaseEventData eventData) => AnimateTo(_pressScale);
    public void OnDeselect(BaseEventData eventData) => AnimateTo(1f);

    private void AnimateTo(float targetScale)
    {
        if (!_isSetup || _target == null) return;

        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(float targetScale)
    {
        float startScale = _target.localScale.x;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            float current = Mathf.Lerp(startScale, targetScale, t);
            _target.localScale = new Vector3(current, current, 1f);
            yield return null;
        }

        _target.localScale = new Vector3(targetScale, targetScale, 1f);
        _coroutine = null;
    }

    private void OnDestroy()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        // Restore scale when handler is removed to avoid stuck-shrunken items.
        if (_target != null)
            _target.localScale = Vector3.one;
    }
}