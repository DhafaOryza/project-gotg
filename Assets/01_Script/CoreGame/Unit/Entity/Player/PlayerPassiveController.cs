using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerBaseEntity))]
public class PlayerPassiveController : MonoBehaviour
{
    private PlayerBaseEntity player;
    private Coroutine currentRegenCouroutine;

    private void Awake()
    {
        player = GetComponent<PlayerBaseEntity>();
    }

    private SkillPassiveSO GetEquippedPassive()
    {
        var skills = player.GetEquippedPassiveSkill();
        return skills as SkillPassiveSO;
    }

    public bool CheckDodgeTrigger()
    {
        var passive = GetEquippedPassive();
        if (passive == null || passive.triggerType != PassiveTriggerType.OnTakeDamage)
            return false;

        float roll = Random.Range(0f, 100f);
        if (roll <= passive.triggerChance)
        {
            // Debug.Log($"<color=green>[PASSIVE] DODGE SUCCESS! ({passive.displayName})</color>");
            return true;
        }
        return false;
    }

    [Header("VFX")]
    [Tooltip("Di-spawn di atas user saat heal terjadi (looping aura).")]
    [SerializeField] private GameObject healVfxPrefab;

    private GameObject healVfxInstance;

    private void SpawnHealVfx()
    {
        if (healVfxPrefab == null || healVfxInstance != null) return;

        healVfxInstance = Instantiate(healVfxPrefab, player.transform.position + Vector3.up * 0.9f, Quaternion.identity);
        healVfxInstance.transform.SetParent(player.transform, true);
    }

    private void DestroyHealVfx()
    {
        if (healVfxInstance == null) return;

        Destroy(healVfxInstance);
        healVfxInstance = null;
    }

    public void HandleEnemyKilled()
    {
        var passive = GetEquippedPassive();
        if (passive == null || passive.triggerType != PassiveTriggerType.OnKillEnemy)
            return;

        if (currentRegenCouroutine != null)
            StopCoroutine(currentRegenCouroutine);

        DestroyHealVfx(); // jangan sampai aura lama dobel dengan regen yang baru
        SpawnHealVfx();

        currentRegenCouroutine = StartCoroutine(RegendHPRoutine(passive.duration, passive.effectValue));
    }

    private IEnumerator RegendHPRoutine(float duration, int healPerSecond)
    {
        float timer = 0f;
        while (timer < duration && !player.IsDead)
        {
            yield return new WaitForSeconds(1f);
            timer += 1f;
            player.Heal(healPerSecond);
            // Debug.Log($"<color=cyan>[PASSIVE REGEN] +{healPerSecond} HP ({timer}/{duration}s)</color>");
        }

        DestroyHealVfx();
        currentRegenCouroutine = null;
    }

    public void StopAllPassiveEffect()
    {
        if (currentRegenCouroutine != null)
        {
            StopCoroutine(currentRegenCouroutine);
            currentRegenCouroutine = null;
        }

        DestroyHealVfx();
    }
}