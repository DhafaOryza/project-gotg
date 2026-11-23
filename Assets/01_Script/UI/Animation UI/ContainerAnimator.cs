using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContainerAnimator : MonoBehaviour
{
    public enum AnimationType
    { 
        PopUp,
        Slide
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
    [SerializeField, Range(0.5f, 1f)] private float _pressScale = 0.85f;
    [SerializeField, Min(0f)] private float _pressDuration = 0.08f;

    public enum SlideDirection 
    { 
        Left, 
        Right, 
        Up, 
        Down 
    }

    private readonly List<RectTransform> _children = new List<RectTransform>();
    private readonly List<PressShrinkHandler> _handlers = new List<PressShrinkHandler>();
    private Coroutine _animationCoroutine;
    private bool _hasPlayed;

    private void OnEnable()
    {
        _hasPlayed = false;
        StartCoroutine(PlayAfterFrame());
    }

    private void OnDisable()
    {
        _hasPlayed = false;

        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    /// <summary>
    /// Dipanggil saat isi container berubah (misal kartu berkurang/bertambah).
    /// </summary>
    /// <param name="replayIntro">
    /// Set TRUE hanya jika ingin memainkan animasi muncul untuk semua kartu (misal saat kartu kembali penuh).
    /// Set FALSE jika hanya ingin memperbarui listener klik/tekan (Press Shrink) tanpa mengganggu kartu lain.
    /// </param>
    public void Refresh(bool replayIntro = false)
    {
        if (!isActiveAndEnabled) return;

        if (replayIntro)
        {
            _hasPlayed = false;
            StartCoroutine(PlayAfterFrame());
        }
        else
        {
            // Hanya pasang ulang event klik/shrink ke kartu yang ada tanpa mengulang animasi intro
            StartCoroutine(RegisterHandlersOnly());
        }
    }

    private IEnumerator PlayAfterFrame()
    {
        if (_hasPlayed) yield break;

        yield return null;
        CollectChildren();
        PlayAnimation();
    }

    private IEnumerator RegisterHandlersOnly()
    {
        yield return null;
        CollectChildren();
    }

    private void CollectChildren()
    {
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

            var selectable = child.GetComponentInChildren<Selectable>(false);
            if (selectable == null) continue;

            var target = selectable.GetComponent<RectTransform>();
            if (target == null) continue;

            // Pasang handler animasi tekan khusus ke kartu ini
            var handler = selectable.gameObject.AddComponent<PressShrinkHandler>();
            handler.Setup(target, _pressScale, _pressDuration);
            _handlers.Add(handler);
        }
    }

    private void PlayAnimation()
    {
        if (_children.Count == 0) return;

        _hasPlayed = true;

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
            SlideDirection.Left  => new Vector2(-_slideStartOffset, 0f),
            SlideDirection.Right => new Vector2( _slideStartOffset, 0f),
            SlideDirection.Up    => new Vector2(0f,  _slideStartOffset),
            SlideDirection.Down  => new Vector2(0f, -_slideStartOffset),
            _ => new Vector2(_slideStartOffset, 0f),
        };
    }

    #endregion
}

/// <summary>
/// Handler mandiri yang ditempelkan ke setiap kartu untuk menangani efek tekan/klik.
/// </summary>
public class PressShrinkHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform _target;
    private float _pressScale;
    private float _duration;
    private Coroutine _coroutine;

    public void Setup(RectTransform target, float pressScale, float duration)
    {
        _target = target;
        _pressScale = pressScale;
        _duration = duration;
    }

    public void OnPointerDown(PointerEventData eventData) => AnimateTo(_pressScale);
    public void OnPointerUp(PointerEventData eventData)   => AnimateTo(1f);

    private void AnimateTo(float targetScale)
    {
        if (_target == null || !gameObject.activeInHierarchy) return;

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

    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        if (_target != null)
            _target.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        if (_target != null)
            _target.localScale = Vector3.one;
    }
}