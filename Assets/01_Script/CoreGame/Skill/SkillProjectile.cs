using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Taruh script ini di prefab skill. Prefab butuh Collider; script ini akan membuatnya
/// jadi trigger dan menambahkan Rigidbody kinematic otomatis (supaya trigger terdeteksi).
/// </summary>
[RequireComponent(typeof(Collider))]
public class SkillProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    [Tooltip("Kalau aktif, projectile menembus musuh dan bisa mengenai banyak musuh.")]
    [SerializeField] private bool piercing = false;

    private BaseEntity _owner;
    private int _damage;
    private float _maxRange;
    private Vector3 _spawnPosition;
    private bool _isPiercing;
    private bool _isLucky;
    private GameObject _damageTextPrefab;
    private readonly HashSet<BaseEntity> alreadyHit = new HashSet<BaseEntity>();

    public void Setup(BaseEntity owner, int damage, float maxRange, bool isAOE ,bool isLucky = false, GameObject damageTextPrefab = null)
    {
        _owner = owner;
        _damage = damage;
        _maxRange = maxRange;
        _spawnPosition = transform.position;
        _isPiercing = isAOE;
        _isLucky = isLucky;
        _damageTextPrefab = damageTextPrefab;
        alreadyHit.Clear();
    }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (!TryGetComponent(out Rigidbody rb))
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Bergerak ke arah "kanan" prefab; arahnya diatur lewat rotasi saat spawn.
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
        float distanceTraveled = Vector3.Distance(_spawnPosition, transform.position);
        if (distanceTraveled >= _maxRange)
            Destroy(gameObject);

        
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponentInParent<BaseEntity>();
        if (target == null || target == _owner || target is PlayerBaseEntity) return;
        if (!alreadyHit.Add(target)) return;             // satu musuh hanya kena sekali

        target.TakeDamage(_damage);

        if (_damageTextPrefab != null)
        {
            Vector3 textspawnPos = target.transform.position + Vector3.up * 1.5f + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
            GameObject textGO = Instantiate(_damageTextPrefab, textspawnPos, Quaternion.identity);
            if (textGO.TryGetComponent(out FloatingDamageText floatText))
                floatText.Setup(_damage, _isLucky);
        }

        if ((target.currentHealth <= 0 || target.IsDead) && _owner is PlayerBaseEntity playerOwner)
        {
            Debug.Log("<color=green>[KILL DETECTED] Proyektil membunuh musuh!</color>");
            playerOwner.OnEnemyKilledByPlayer();
        }

        if (!_isPiercing)
            Destroy(gameObject);
    }
}