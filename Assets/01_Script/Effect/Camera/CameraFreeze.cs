using System.Collections;
using UnityEngine;

public class CameraFreeze : MonoBehaviour
{
    [Header("Freeze Settings")]
    [SerializeField, Min(0f)] private float _defaultDuration = 0.05f;
    [SerializeField, Range(0f, 1f)] private float _freezeTimeScale = 0f;

    private Coroutine _freezeCoroutine;

    /// <summary>
    /// Triggers a time freeze with the default duration.
    /// </summary>
    public void Freeze()
    {
        Freeze(_defaultDuration);
    }

    /// <summary>
    /// Triggers a time freeze for the given duration (in real time).
    /// </summary>
    public void Freeze(float duration)
    {
        if (!gameObject.activeInHierarchy) return;

        if (_freezeCoroutine != null)
            StopCoroutine(_freezeCoroutine);

        _freezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = _freezeTimeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        _freezeCoroutine = null;
    }
}