using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Menjalankan VFX Graph + semua ParticleSystem pada prefab HealSpell_Enhanced secara bersamaan.
/// Otomatis main saat prefab aktif. Aktifkan Loop kalau efeknya dipakai sebagai aura yang terus menyala.
/// </summary>
public class VFXScript : MonoBehaviour
{
    [Tooltip("Ulangi efek terus-menerus (mis. aura heal dari passive). Matikan untuk sekali main.")]
    [SerializeField] private bool loop = false;

    [Tooltip("Jeda antar pengulangan (detik) saat Loop aktif. Satu putaran efek berdurasi sekitar 2.5 detik.")]
    [SerializeField] private float loopInterval = 2.4f;

    private VisualEffect vfx;
    private ParticleSystem[] particleSystems;
    private float timer;

    private void Awake()
    {
        vfx = GetComponentInChildren<VisualEffect>(true);
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        Play();
    }

    private void Update()
    {
        if (!loop) return;

        timer += Time.deltaTime;
        if (timer >= loopInterval)
            Play();
    }

    public void SetLoop(bool value)
    {
        loop = value;
    }

    /// <summary>Mulai ulang seluruh efek dari awal.</summary>
    public void Play()
    {
        timer = 0f;

        if (vfx != null)
        {
            vfx.Reinit();
            vfx.Play();
        }

        if (particleSystems == null) return;

        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }
}
