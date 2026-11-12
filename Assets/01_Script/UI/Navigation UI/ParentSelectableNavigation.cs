using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lets a parent container assign explicit navigation between its active child Selectables.
/// </summary>
public class ParentSelectableNavigation : MonoBehaviour
{
    [SerializeField] private bool includeNestedSelectables = true;

    [Header("External Navigation Roots")]
    [SerializeField] private List<GameObject> upRoot;
    [SerializeField] private List<GameObject> downRoot;
    [SerializeField] private List<GameObject> leftRoot;
    [SerializeField] private List<GameObject> rightRoot;

    private Coroutine refreshRoutine;

    private void OnEnable()
    {
        RefreshNavigation();
    }

    private void OnDisable()
    {
        if (refreshRoutine == null) return;

        StopCoroutine(refreshRoutine);
        refreshRoutine = null;
    }

    private void OnTransformChildrenChanged()
    {
        if (isActiveAndEnabled)
            RefreshNavigation();
    }

    /// <summary>
    /// Rebuilds child navigation after generated UI has finished laying out.
    /// </summary>
    public void RefreshNavigation()
    {
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);

        refreshRoutine = StartCoroutine(RefreshAfterLayout());
    }

    private IEnumerator RefreshAfterLayout()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        var selectables = GetChildSelectables();
        if (selectables.Count > 0)
            AssignNavigation(selectables);

        refreshRoutine = null;
    }

    private List<Selectable> GetChildSelectables()
    {
        var result = new List<Selectable>();

        if (includeNestedSelectables)
        {
            foreach (var selectable in GetComponentsInChildren<Selectable>(false))
            {
                if (IsUsable(selectable))
                    result.Add(selectable);
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                var selectable = child.GetComponent<Selectable>();
                if (IsUsable(selectable))
                    result.Add(selectable);
            }
        }

        return result;
    }

    private bool IsUsable(Selectable selectable)
    {
        return selectable != null && selectable.transform != transform &&
               selectable.isActiveAndEnabled && selectable.IsInteractable();
    }

    private void AssignNavigation(List<Selectable> selectables)
    {
        var canvas = GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : null;

        foreach (var source in selectables)
        {
            Vector2 sourcePosition = GetScreenPosition(source.transform, uiCamera);
            var navigation = source.navigation;

            ApplyDirection(selectables, source, sourcePosition, Vector2.up, upRoot, uiCamera,
                ref navigation, NavigationDirection.Up);
            ApplyDirection(selectables, source, sourcePosition, Vector2.down, downRoot, uiCamera,
                ref navigation, NavigationDirection.Down);
            ApplyDirection(selectables, source, sourcePosition, Vector2.left, leftRoot, uiCamera,
                ref navigation, NavigationDirection.Left);
            ApplyDirection(selectables, source, sourcePosition, Vector2.right, rightRoot, uiCamera,
                ref navigation, NavigationDirection.Right);

            navigation.mode = Navigation.Mode.Explicit;
            source.navigation = navigation;
        }
    }

    private enum NavigationDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private static void ApplyDirection(
        List<Selectable> localSelectables,
        Selectable source,
        Vector2 sourcePosition,
        Vector2 direction,
        List<GameObject> externalRoots,
        Camera uiCamera,
        ref Navigation navigation,
        NavigationDirection navigationDirection)
    {
        Selectable target = FindNearestInDirection(
            localSelectables, source, sourcePosition, direction, uiCamera);

        if (target == null)
            target = FindSelectableInRoots(externalRoots, source, sourcePosition, direction, uiCamera);

        bool hasConfiguredRoots = externalRoots != null && externalRoots.Exists(root => root != null);
        // Clear an outdated link if roots are configured but none currently has an active Selectable.
        if (target == null && !hasConfiguredRoots)
            return;

        switch (navigationDirection)
        {
            case NavigationDirection.Up:
                navigation.selectOnUp = target;
                break;
            case NavigationDirection.Down:
                navigation.selectOnDown = target;
                break;
            case NavigationDirection.Left:
                navigation.selectOnLeft = target;
                break;
            case NavigationDirection.Right:
                navigation.selectOnRight = target;
                break;
        }
    }

    private static Selectable FindSelectableInRoots(
        List<GameObject> roots,
        Selectable source,
        Vector2 sourcePosition,
        Vector2 direction,
        Camera uiCamera)
    {
        var activeCandidates = new List<Selectable>();
        var addedCandidates = new HashSet<Selectable>();

        if (roots == null) return null;

        foreach (var root in roots)
        {
            if (root == null || !root.activeInHierarchy) continue;

            foreach (var candidate in root.GetComponentsInChildren<Selectable>(false))
            {
                if (IsUsableCandidate(candidate, source) && addedCandidates.Add(candidate))
                    activeCandidates.Add(candidate);
            }
        }

        if (activeCandidates.Count == 0) return null;

        // Prefer a target that is physically in the requested direction within that root.
        var directionalTarget = FindNearestInDirection(
            activeCandidates, source, sourcePosition, direction, uiCamera);
        if (directionalTarget != null)
            return directionalTarget;

        // The root itself identifies the direction; if its child is offset, choose its nearest Selectable.
        Selectable nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (var candidate in activeCandidates)
        {
            float distance = (GetScreenPosition(candidate.transform, uiCamera) - sourcePosition).sqrMagnitude;
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearest = candidate;
        }

        return nearest;
    }

    private static Selectable FindNearestInDirection(
        List<Selectable> selectables,
        Selectable source,
        Vector2 sourcePosition,
        Vector2 direction,
        Camera uiCamera)
    {
        const float minimumDirectionDot = 0.5f;
        Selectable nearest = null;
        float bestScore = float.PositiveInfinity;

        foreach (var candidate in selectables)
        {
            if (candidate == source) continue;

            Vector2 candidatePosition = GetScreenPosition(candidate.transform, uiCamera);
            Vector2 offset = candidatePosition - sourcePosition;
            float distance = offset.magnitude;
            if (distance <= Mathf.Epsilon) continue;

            float directionDot = Vector2.Dot(offset / distance, direction);
            if (directionDot < minimumDirectionDot) continue;

            float score = distance / directionDot;
            if (score >= bestScore) continue;

            bestScore = score;
            nearest = candidate;
        }

        return nearest;
    }

    private static Vector2 GetScreenPosition(Transform target, Camera fallbackCamera)
    {
        var canvas = target.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : fallbackCamera;

        return RectTransformUtility.WorldToScreenPoint(camera, target.position);
    }

    private static bool IsUsableCandidate(Selectable candidate, Selectable source)
    {
        return candidate != null && candidate != source && candidate.isActiveAndEnabled && candidate.IsInteractable();
    }
}
