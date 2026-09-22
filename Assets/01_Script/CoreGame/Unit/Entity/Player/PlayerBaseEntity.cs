using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseEntity : BaseEntity
{
    // ---------- Skill Loadout (Preparation: "Pilih 3-5 skill dari koleksi") ----------
    [Header("Skill Loadout (3-5 skill)")]
    [SerializeField] private List<SkillDataSO> equippedSkills = new List<SkillDataSO>();
    private SkillSlotRuntime[] skillSlots;

    // ---------- Consumable Loadout (Consumable Q/E) ----------
    [Header("Consumable Loadout")]
    [SerializeField] private List<ConsumableDataSO> equippedConsumables = new List<ConsumableDataSO>();
    [SerializeField] private int defaultConsumableQuantity = 2;
    private ConsumableSlotRuntime[] consumableSlots;

    // ---------- Movement (WASD) ----------
    [Header("Runtime Movement State")]
    public Vector2 MoveInput { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.down;

    // ---------- State ----------
    public bool IsKO => IsDead; // Player KO != Lumbung hancur, jadi dipisah dari "game over"
    public bool CanAct => !IsKO;

    // ---------- Events untuk UI (damage number, cooldown radial, KO transition) ----------
    public event Action<int, float> OnSkillCooldownUpdated;   // slotIndex, normalizedCooldown (0-1, 0 = ready)
    public event Action<int> OnSkillUsed;                     // slotIndex
    public event Action<int, string> OnSkillUseFailed;        // slotIndex, alasan
    public event Action<int, int> OnConsumableQuantityChanged; // slotIndex, quantity
    public event Action OnPlayerKO;
    public event Action OnPlayerRecovered;

    protected override void Awake()
    {
        base.Awake();
        BuildSkillSlots();
        BuildConsumableSlots();

        OnDied += HandlePlayerKO;
    }

    private void OnDestroy()
    {
        OnDied -= HandlePlayerKO;
    }

    private void Update()
    {
        TickSkillCooldowns(Time.deltaTime);

        if (!CanAct) return; // KO -> tidak bisa gerak/skill/consumable

        ReadMovementInput();
        ReadSkillInput();
        ReadConsumableInput();
    }

    // ============================================================
    // MOVEMENT (Controls: WASD Move, Mouse Aim/Direction)
    // ============================================================

    private void ReadMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(h, v).normalized;

        if (MoveInput.sqrMagnitude > 0.0001f)
            AimDirection = MoveInput; // fallback aim kalau belum ada mouse-aim terpisah

        Vector3 delta = new Vector3(MoveInput.x, MoveInput.y, 0f) * entityData.moveSpeed * Time.deltaTime;
        transform.position += delta; // ganti dengan Rigidbody2D/CharacterController sesuai kebutuhan movement kamu
    }

    public void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
            AimDirection = direction.normalized;
    }

    // ============================================================
    // SKILL SYSTEM (No Normal Attack — semua kombat berbasis skill cooldown)
    // ============================================================

    private void BuildSkillSlots()
    {
        skillSlots = new SkillSlotRuntime[equippedSkills.Count];
        for (int i = 0; i < equippedSkills.Count; i++)
        {
            skillSlots[i] = new SkillSlotRuntime { data = equippedSkills[i], cooldownRemaining = 0f };
        }
    }

    private void ReadSkillInput()
    {
        for (int i = 0; i < skillSlots.Length && i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                TryUseSkill(i);
        }
    }

    private void TickSkillCooldowns(float deltaTime)
    {
        if (skillSlots == null) return;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            var slot = skillSlots[i];
            if (slot.data == null || slot.cooldownRemaining <= 0f) continue;

            slot.cooldownRemaining = Mathf.Max(0f, slot.cooldownRemaining - deltaTime);

            float normalized = slot.data.cooldown > 0f
                ? slot.cooldownRemaining / (slot.data.cooldown * Mathf.Max(0.01f, entityData.skillCooldownMultiplier))
                : 0f;

            OnSkillCooldownUpdated?.Invoke(i, normalized);
        }
    }

    public bool TryUseSkill(int slotIndex)
    {
        if (!CanAct) { OnSkillUseFailed?.Invoke(slotIndex, "Player KO"); return false; }
        if (skillSlots == null || slotIndex < 0 || slotIndex >= skillSlots.Length) return false;

        var slot = skillSlots[slotIndex];
        if (slot.data == null) { OnSkillUseFailed?.Invoke(slotIndex, "Slot kosong"); return false; }
        if (slot.cooldownRemaining > 0f) { OnSkillUseFailed?.Invoke(slotIndex, "Cooldown"); return false; }

        slot.data.Activate(this);
        slot.cooldownRemaining = slot.data.cooldown * Mathf.Max(0.01f, entityData.skillCooldownMultiplier);

        OnSkillUsed?.Invoke(slotIndex);
        return true;
    }

    // Dipanggil dari Preparation Screen (pilih skill & urutan slot sebelum malam)
    public void SetSkillLoadout(List<SkillDataSO> skills)
    {
        if (skills == null || skills.Count < 3 || skills.Count > 5)
        {
            Debug.LogWarning("[PlayerBaseEntity] Skill loadout harus 3-5 skill sesuai desain.");
            return;
        }

        equippedSkills = new List<SkillDataSO>(skills);
        BuildSkillSlots();
    }

    public float GetSkillCooldownRemaining(int slotIndex)
    {
        if (skillSlots == null || slotIndex < 0 || slotIndex >= skillSlots.Length) return 0f;
        return skillSlots[slotIndex].cooldownRemaining;
    }

    // ============================================================
    // CONSUMABLE SYSTEM (Controls: Q/E, dipakai "pada situasi darurat")
    // ============================================================

    private void BuildConsumableSlots()
    {
        consumableSlots = new ConsumableSlotRuntime[equippedConsumables.Count];
        for (int i = 0; i < equippedConsumables.Count; i++)
        {
            consumableSlots[i] = new ConsumableSlotRuntime
            {
                data = equippedConsumables[i],
                quantity = defaultConsumableQuantity
            };
        }
    }

    private void ReadConsumableInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) UseConsumable(0);
        if (Input.GetKeyDown(KeyCode.E)) UseConsumable(1);
    }

    public bool UseConsumable(int slotIndex)
    {
        if (!CanAct) return false;
        if (consumableSlots == null || slotIndex < 0 || slotIndex >= consumableSlots.Length) return false;

        var slot = consumableSlots[slotIndex];
        if (slot.data == null || slot.quantity <= 0) return false;

        slot.data.Use(this);
        slot.quantity--;

        OnConsumableQuantityChanged?.Invoke(slotIndex, slot.quantity);
        return true;
    }

    // Dipanggil dari Preparation Screen ("Memilih consumable")
    public void SetConsumableLoadout(List<ConsumableDataSO> consumables, int quantityEach)
    {
        equippedConsumables = new List<ConsumableDataSO>(consumables);
        defaultConsumableQuantity = quantityEach;
        BuildConsumableSlots();
    }

    public void RestockConsumables(int quantityEach)
    {
        if (consumableSlots == null) return;
        for (int i = 0; i < consumableSlots.Length; i++)
        {
            consumableSlots[i].quantity = quantityEach;
            OnConsumableQuantityChanged?.Invoke(i, quantityEach);
        }
    }

    // ============================================================
    // KO / RECOVERY (Result: "Player KO tetapi lumbung masih bertahan")
    // ============================================================

    private void HandlePlayerKO()
    {
        MoveInput = Vector2.zero;
        OnPlayerKO?.Invoke();
        // TODO: trigger animasi KO, disable collider serangan, dsb sesuai kebutuhanmu
    }

    // Dipanggil oleh sistem Morning Phase / Day-Night Manager
    public override void RecoverForMorning()
    {
        base.RecoverForMorning();
        OnPlayerRecovered?.Invoke();
    }

    // ============================================================
    // Runtime slot classes (bukan MonoBehaviour, cukup plain data)
    // ============================================================

    [Serializable]
    private class SkillSlotRuntime
    {
        public SkillDataSO data;
        public float cooldownRemaining;
    }

    [Serializable]
    private class ConsumableSlotRuntime
    {
        public ConsumableDataSO data;
        public int quantity;
    }
}