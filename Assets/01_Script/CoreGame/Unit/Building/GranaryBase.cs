using UnityEngine;
using UnityEngine.Events;

public class GranaryBase : BuildingBase
{
    [Header ("Granary Events")]
    public UnityEvent OnGranaryDestroyed;

    [Header ("Visual FeedBack")]
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private Color DestroyedColor = Color.red;

    private void Reset()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
    }
    protected override void Destroy()
    {
        base.Destroy();

        if (_sr != null)
            _sr.color = DestroyedColor;
        Debug.Log("<color=red>[Granary]</color> Granary Hancur! Trigger Lose Condition.");

        GameEndUIManager uiManager = FindAnyObjectByType<GameEndUIManager>();
            if (uiManager != null)
                uiManager.ShowLosePanel();
            else
                Debug.LogError("[Granary] GameEndUIManager tidak ditemukan di scene!");
        
        OnGranaryDestroyed?.Invoke();
    }
}