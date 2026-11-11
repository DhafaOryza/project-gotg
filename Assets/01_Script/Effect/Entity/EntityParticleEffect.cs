using UnityEngine;

public class EntityParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem hitParticlePrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);

    private BaseEntity baseEntity;

    private void Awake()
    {
        baseEntity = GetComponent<BaseEntity>();
    }

    private void OnEnable()
    {
        if (baseEntity != null)
        {
            baseEntity.OnDamaged += PlayHitParticle;
        }
    }

    private void OnDisable()
    {
        if (baseEntity != null)
        {
            baseEntity.OnDamaged -= PlayHitParticle;
        }
    }

    /// <summary>
    /// Dipanggil otomatis saat event OnDamaged pada BaseEntity terpicu.
    /// </summary>
    private void PlayHitParticle(int damageAmount)
    {
        if (hitParticlePrefab == null) return;

        Vector3 spawnPosition = transform.position + spawnOffset;
        
        // Instansiasi prefab partikel pada posisi entity
        ParticleSystem particleInstance = Instantiate(hitParticlePrefab, spawnPosition, Quaternion.identity);
        particleInstance.Play();

        // Hancurkan objek partikel secara otomatis setelah durasinya selesai
        var mainModule = particleInstance.main;
        float totalDuration = mainModule.duration + mainModule.startLifetime.constantMax;
        Destroy(particleInstance.gameObject, totalDuration);
    }
}