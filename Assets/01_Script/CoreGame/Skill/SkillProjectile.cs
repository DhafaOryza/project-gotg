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

    private BaseEntity owner;
    private int damage;
    private readonly HashSet<BaseEntity> alreadyHit = new HashSet<BaseEntity>();

    public void Setup(BaseEntity owner, int damage)
    {
        this.owner = owner;
        this.damage = damage;
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
    }

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponentInParent<BaseEntity>();
        if (target == null || target == owner) return;
        if (target is PlayerBaseEntity) return;          // tidak melukai player
        if (!alreadyHit.Add(target)) return;             // satu musuh hanya kena sekali

        target.TakeDamage(damage);

        if (!piercing)
            Destroy(gameObject);
    }
}