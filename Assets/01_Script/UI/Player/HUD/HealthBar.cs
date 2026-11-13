using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar berbasis Image bertipe Filled. currentHp = bar utama, ghostHp = bar "jejak damage"
/// yang turun pelan di belakangnya. Bisa dipasang ke apa pun yang mengimplementasi IHealthSource
/// (BaseEntity maupun BuildingBase).
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image currentHp;
    [SerializeField] private Image ghostHp;

    [Tooltip("Opsional: teks seperti 100/100.")]
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Ghost Bar")]
    [Tooltip("Jeda (detik) sebelum bar ghost mulai turun setelah kena damage.")]
    [SerializeField] private float ghostDelay = 0.4f;

    [Tooltip("Kecepatan turun bar ghost (porsi bar per detik).")]
    [SerializeField] private float ghostSpeed = 0.6f;

    [Header("Shake")]
    [SerializeField] private float _shakeDuration  = 0.2f;
    [SerializeField] private float _shakeMagnitude = 5f;

    [Header("Bar Color")]
    [SerializeField] private Color _colorFull    = Color.green;
    [SerializeField] private Color _colorMid     = Color.yellow;
    [SerializeField] private Color _colorLow     = Color.red;
    [SerializeField] private float _midThreshold = 0.5f;
    [SerializeField] private float _lowThreshold = 0.25f;

    private RectTransform _rectTransform;
    private Vector3 _originalPosition;

    private IHealthSource source;
    private float targetFill = 1f;
    private float ghostTimer;

    // Interface tidak mengenal "Unity null" (objek yang sudah di-Destroy), jadi dicek manual.
    private bool HasSource =>
        source != null && !(source is Object unityObject && unityObject == null);

    /// <summary>
    /// Pasang bar ke sumber nyawa. Bisa BaseEntity ataupun BuildingBase:
    ///   healthBar.Initialize(playerEntity);
    ///   healthBar.Initialize(building);
    /// </summary>
    public void Initialize(IHealthSource newSource)
    {
        Unbind();

        source = newSource;
        if (!HasSource)
        {
            source = null;
            return;
        }

        source.OnHealthChanged += HandleHealthChanged;

        Refresh(source.CurrentHealth, source.MaxHealth);

        // Saat pertama kali dipasang, ghost langsung mengikuti (tanpa efek turun).
        SnapGhost();
    }

    private void Awake()
    {
        _rectTransform   = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        // Kalau bar dinonaktifkan lalu diaktifkan lagi, sinkronkan ulang tanpa efek turun.
        if (HasSource)
        {
            Refresh(source.CurrentHealth, source.MaxHealth);
            SnapGhost();
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void Update()
    {
        AnimateGhost();
    }

    private void Unbind()
    {
        if (source != null)
            source.OnHealthChanged -= HandleHealthChanged;

        source = null;
    }

    private void HandleHealthChanged(int current, int max)
    {
        Refresh(current, max);
    }

    private void Refresh(int current, int max)
    {
        float fill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (fill < targetFill)
        {
            ShakeBar();
        }

        targetFill = fill;

        if (currentHp != null)
            currentHp.fillAmount = fill;

        if (ghostHp != null)
        {
            if (fill < ghostHp.fillAmount)
                ghostTimer = ghostDelay;        // kena damage: ghost menunggu lalu turun
            else
                ghostHp.fillAmount = fill;      // heal / repair: ghost langsung naik menyusul
        }

        if (hpText != null)
            hpText.text = $"{current}/{max}";

        UpdateBarColor(fill);
    }

    private void SnapGhost()
    {
        if (ghostHp != null)
            ghostHp.fillAmount = targetFill;
    }

    private void AnimateGhost()
    {
        if (ghostHp == null || ghostHp.fillAmount <= targetFill) return;

        if (ghostTimer > 0f)
        {
            ghostTimer -= Time.deltaTime;
            return;
        }

        ghostHp.fillAmount = Mathf.MoveTowards(ghostHp.fillAmount, targetFill, ghostSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Triggers a shake effect on the health bar when damage is received.
    /// Only shakes when health decreases, not when healing.
    /// </summary>
    private void ShakeBar()
    {
        StopCoroutine(nameof(DoShake));
        StartCoroutine(nameof(DoShake));
    }
    
    private IEnumerator DoShake()
    {
        float elapsed = 0f;
    
        while (elapsed < _shakeDuration)
        {
            elapsed += Time.deltaTime;
    
            // random offset ke kanan dan kiri
            float offsetX = Random.Range(-_shakeMagnitude, _shakeMagnitude);
            float offsetY = Random.Range(-_shakeMagnitude, _shakeMagnitude);
    
            _rectTransform.anchoredPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
    
        // kembalikan ke posisi awal
        _rectTransform.anchoredPosition = _originalPosition;
    }

    /// <summary>
    /// Updates the health bar color based on current fill percentage.
    /// Green = full, Yellow = mid, Red = low.
    /// Uses Lerp for smooth transition between colors.
    /// </summary>
    private void UpdateBarColor(float fill)
    {
        if (currentHp == null) return;

        Color targetColor;

        if (fill > _midThreshold)
        {
            // lerp dari kuning ke hijau
            float t       = (fill - _midThreshold) / (1f - _midThreshold);
            targetColor   = Color.Lerp(_colorMid, _colorFull, t);
        }
        else if (fill > _lowThreshold)
        {
            // lerp dari merah ke kuning
            float t       = (fill - _lowThreshold) / (_midThreshold - _lowThreshold);
            targetColor   = Color.Lerp(_colorLow, _colorMid, t);
        }
        else
        {
            targetColor = _colorLow;
        }

        currentHp.color = targetColor;
    }
}