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
}