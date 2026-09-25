using UnityEngine;

public class EnemyAttacker : EnemyBaseEntity
{
    public override void PerformAction(GameObject target)
    {
        if (target == null)
        {
            Debug.Log("Enemy hilang !");
            return;
        }

        if (target == gameObject)
        {
            Debug.LogWarning("Target adalah diri sendiri! Batal menyerang.");
            return;
        }

        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            int damage = enemyData != null ? Mathf.RoundToInt(enemyData.executeDamage) : 10;
            damageable.TakeDamage(damage);
            Debug.Log($"⚔️ [{gameObject.name}] menyerang [{target.name}] sebesar {damage} damage!");
        }
    }
}