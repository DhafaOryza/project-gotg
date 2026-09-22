using UnityEngine;
public class EnemyCore : BaseEntity
{
    protected EnemyMovement movement;
    [SerializeField] private Transform targetTransform;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<EnemyMovement>();
    }

    private void Start()
    {
        // Cari Lumbung otomatis jika slot target di Inspector kosong
        if (targetTransform == null)
        {
            // GranaryCore granary = FindFirstObjectByType<GranaryCore>();
            // if (granary != null)
            // {
            //     targetTransform = granary.transform;
            // }
        }

        if (targetTransform != null && movement != null)
        {
            movement.SetTarget(targetTransform);
        }
    }

    protected override void Die()
    {
        if (movement != null)
        {
            movement.SetCanMove(false);
        }

        base.Die();
    }
}