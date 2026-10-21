using Unity.Mathematics;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float stopDistance = 0.1f;

    [SerializeField] private bool holdToFollow = true;

    [Header("Optional")]
    [SerializeField] private GameObject clickMarkerPrefab;

    [Header ("Ghost Preview & Snapping Settings")]        
    private SkillGhostPreview currentGhost;

    [Header ("Skill Settings & AttackPoint")]
    [Range(0.05f, 1f)]
    [SerializeField] private float slowMoScale = 0.5f;
    [SerializeField] private Transform attackPoint;

    [Header("Skill Keys")]
    public KeyCode skill1 = KeyCode.Q;
    public KeyCode skill2 = KeyCode.W;
    public KeyCode skill3 = KeyCode.E;
    public KeyCode skill4 = KeyCode.R;

    [Header("Stop")]
    public KeyCode stopAct = KeyCode.S;

    [Header("Interact Key")]
    public KeyCode interact = KeyCode.F;

    [Header("Interact Settings")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactLayers = ~0;

    private const int SkillSlotCount = 4;

    private PlayerBaseEntity playerEntity;
    private GameObject playerObject;
    private Rigidbody playerRb;
    private Camera cam;

    private float targetX;
    private bool hasTarget;

    // Buffer untuk OverlapSphereNonAlloc supaya tidak membuat array baru tiap interact.
    private readonly Collider[] interactBuffer = new Collider[16];
    private bool suppressHeldFollow = false;
    // Setelah Stop(), klik kanan yang masih ditahan tidak langsung menggerakkan player lagi.
    // Flag ini hilang saat tombol mouse dilepas atau ada klik baru.
   

    //Tracking Mode
    private bool isAiming = false;
    private int activeAimSlot = -1;
    private Vector2 currentAimDirection = Vector2.right;

    public bool IsMoving => hasTarget;
    public bool IsAiming => isAiming;
    public Vector2 CurrentAimDirection => currentAimDirection;

    public void Setup()
    {
        var _currentPlayer = GameManager.Instance?.levelSpawnerManager.GetCurrentPlayer();
        if (_currentPlayer == null)
        {
            Debug.LogError("PlayerController: current player tidak ditemukan.");
            return;
        }

        playerEntity = _currentPlayer;
        // GameObject bukan Component, jadi GetComponent<GameObject>() selalu null.
        playerObject = _currentPlayer.gameObject;
        playerRb = _currentPlayer.GetComponent<Rigidbody>();
        cam = Camera.main;

        if (attackPoint == null)
        {
            attackPoint = playerObject.transform.Find("AttackPoint");
            if (attackPoint == null)
                attackPoint = playerObject.transform.Find("attackpoint");

            if (attackPoint == null)
            {
                Debug.LogWarning("[PlayerController] Child AttackPoint TIDAK DITEMUKAN di Player! Skill akan keluar dari pivot kaki.");
                attackPoint = playerObject.transform;
            }
            else
            {
                Debug.Log($"[PlayerController] Berhasil mengaitkan AttackPoint di posisi Y: {attackPoint.localPosition.y}");
            }
            
        }
    }

    private void Update()
    {
        if (playerEntity == null || playerRb == null || cam == null) return;

        // Player yang knockdown atau mati tidak boleh bergerak maupun memakai skill.
        if (!playerEntity.CanAct)
        {
            hasTarget = false;
            return;
        }

        // Tombol stop: berhenti saat itu juga, input lain di frame ini diabaikan.
        if (ReadKeyDown(stopAct))
        {
            Stop();
            return;
        }

        if (ReadKeyDown(interact))
            TryInteract();

        HandleSkillInput();
        HandleMovementInput();
    }

    // Cari IInteractable terdekat dalam interactRange, lalu panggil Interact()-nya.
    private void TryInteract()
    {
        Vector3 playerPos = playerRb.position;

        int count = Physics.OverlapSphereNonAlloc(
            playerPos, interactRange, interactBuffer, interactLayers,
            QueryTriggerInteraction.Collide);

        IInteractable nearest = null;
        float nearestSqrDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var interactable = interactBuffer[i].GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            // Jarak ke titik terdekat collider (bukan ke pivot), supaya bangunan lebar tetap akurat.
            Vector3 closest = interactBuffer[i].bounds.ClosestPoint(playerPos);
            float sqrDist = ((Vector2)closest - (Vector2)playerPos).sqrMagnitude;

            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = interactable;
            }
        }

        nearest?.Interact(playerEntity);
    }

    private void HandleMovementInput()
    {   
        if (isAiming) return;

        bool pressed = ReadRightMouseDown();
        bool rawHeld = ReadRightMouseHeld();

        if (!rawHeld || pressed)
            suppressHeldFollow = false;

        bool held = holdToFollow && rawHeld && !suppressHeldFollow;

        if (!pressed && !held) return;

        if (TryGetMouseWorldPosition(out Vector3 mouseworldPosition))
        {
            targetX = mouseworldPosition.x;
            hasTarget = true;

            if (pressed && clickMarkerPrefab != null)
            {
                Instantiate(clickMarkerPrefab,
                    new Vector3(mouseworldPosition.x, playerRb.position.y, playerRb.position.z),
                    Quaternion.identity);
            }
        }
    }

    // Tombol skill1..skill4 dipetakan berurutan ke slot 0..3 di equippedSkills.
    private void HandleSkillInput()
    {
        if (isAiming)
        {
            CalculateAimDirection();

            if (ReadRightMouseDown())
            {
                CancelAiming();
                suppressHeldFollow = true;
                return;
            }

            var releaseKey = ReadKeyUp(GetSkillKey(activeAimSlot));
            var leftClick = ReadLeftMouseDown();

            if (releaseKey || leftClick)
                ExecuteActiveSkill();

            return ;
        }

        for (int slot = 0; slot < SkillSlotCount; slot++)
        {
            KeyCode key = GetSkillKey(slot);
            if (ReadKeyDown(key))
            {
                if (playerEntity.GetSkillCooldownRemaining(slot) <= 0f)
                {
                   var skillData = playerEntity.GetEquippedSkillData(slot);
                   if (skillData == null) continue;

                    if (skillData.targetType == SkillTargetType.Self)
                        playerEntity.TryUseSkill(slot, attackPoint, Vector2.right);
                    else    
                        StartAiming(slot);
                    
                    break;
                }
                else
                {
                    Debug.Log($"[Skill] slot {slot} masih cooldown");
                }
                
            }
        }
    }

#region  Aiming System
    private void StartAiming(int slotIndex)
    {
        isAiming = true;
        activeAimSlot = slotIndex;
        Stop();

        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        var skilldata = playerEntity.GetEquippedSkillData(slotIndex);
        if (skilldata != null)
            SpawnGhost(skilldata);
    }

    private void CalculateAimDirection()
    {
        if (!TryGetMouseWorldPosition(out Vector3 mouseworldpos)) return;
        SkillDataSO skillData = playerEntity.GetEquippedSkillData(activeAimSlot);
        if (skillData == null) return;

        float mouseDirectionX = mouseworldpos.x - playerRb.position.x;
        if (attackPoint != null && attackPoint != playerObject.transform)
        {
            Vector3 localpos = attackPoint.localPosition;
            float absX = Mathf.Abs(localpos.x);

            localpos.x = (mouseDirectionX >= 0f)? absX : -absX;
            attackPoint.localPosition = localpos;
        }

        Vector3 origin = attackPoint != null ? attackPoint.position : playerRb.position;
        Vector2 dir = new Vector2(mouseworldpos.x - origin.x, mouseworldpos.y - origin.y);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle = (dir.x >= 0f) ? 0f : 180f;

        float rad = angle * Mathf.Deg2Rad;
        currentAimDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
        if (currentGhost != null)
        {
            Vector3 ghostPosition = origin + (Vector3)(currentAimDirection * skillData.maxRange);
            Quaternion ghostRotation = Quaternion.Euler(0f, 0f, angle);

            currentGhost.SetPositionandRotation(ghostPosition, ghostRotation);
        }
    }

    private void ExecuteActiveSkill()
    {
        int slot = activeAimSlot;
        DestroyGhost();
        ResetTimeScale();
        isAiming = false;
        activeAimSlot = -1;
        playerEntity.TryUseSkill(slot, attackPoint, currentAimDirection);
    }

    private void CancelAiming()
    {
        DestroyGhost();
        ResetTimeScale();
        isAiming = false;
        activeAimSlot = -1;
    }

    private void SpawnGhost(SkillDataSO data)
    {
        if (data.ghostPreviewPrefab == null) return;

        GameObject ghostGO = Instantiate(data.ghostPreviewPrefab, attackPoint.position, Quaternion.identity);
        currentGhost = ghostGO.GetComponent<SkillGhostPreview>();
    }

    private void DestroyGhost()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost.gameObject);
            currentGhost = null;
        }
    }

#endregion

    private void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    private KeyCode GetSkillKey(int slot)
    {
        switch (slot)
        {
            case 0: return skill1;
            case 1: return skill2;
            case 2: return skill3;
            default: return skill4;
        }
    }

    private void FixedUpdate()
    {
        if (playerRb == null || playerEntity == null) return;

        Vector3 vel = playerRb.linearVelocity; // Unity 6. Di versi lama pakai .velocity

        if (!hasTarget)
        {
            vel.x = 0f;
            playerRb.linearVelocity = vel;
            return;
        }

        float delta = targetX - playerRb.position.x;

        if (Mathf.Abs(delta) <= stopDistance)
        {
            hasTarget = false;
            vel.x = 0f;
        }
        else
        {
            var moveSpeed = playerEntity.entityData.moveSpeed;
            vel.x = Mathf.Sign(delta) * moveSpeed;
        }

        // Y tetap dibiarkan supaya gravitasi dan lompat tidak terganggu.
        playerRb.linearVelocity = vel;
    }

    // Hentikan pergerakan seketika: hapus target dan nolkan kecepatan horizontal sekarang juga.
    public void Stop()
    {
        hasTarget = false;
        suppressHeldFollow = true;

        if (playerRb != null)
        {
            Vector3 vel = playerRb.linearVelocity;
            vel.x = 0f;
            playerRb.linearVelocity = vel;
        }
    }

    public void MoveTo(float worldX)
    {
        targetX = worldX;
        hasTarget = true;
    }

    // Ubah posisi mouse jadi posisi dunia di bidang tempat player berada.
    private bool TryGetMouseWorldPosition(out Vector3 worldpos)
    {
        worldpos = Vector3.zero;

        Vector2 screenPos = ReadMousePosition();
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Bidang tegak lurus sumbu Z yang melewati player (bidang gerak 2D-nya).
        Plane plane = new Plane(Vector3.back, playerRb.position);

        if (plane.Raycast(ray, out float enter))
        {
            worldpos = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

    // Input: jalan di Input System baru maupun yang lama.

#region  Input helper

    private bool ReadLeftMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
    return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else 
    return Input.GetMouseButtonDown(0);
#endif
    }

    private bool ReadRightMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
    return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else 
    return Input.GetMouseButtonDown(1);
#endif 
    }

   private bool ReadRightMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    private Vector2 ReadMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    // KeyCode adalah tipe Input lama. Di Input System baru, namanya dicocokkan ke enum Key
    // (Q, W, E, R, Space, dll. cocok langsung; Alpha1 -> Digit1 ditangani di bawah).
    private bool ReadKeyDown(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return false;

        string keyName = keyCode.ToString().Replace("Alpha", "Digit");
        if (System.Enum.TryParse(keyName, out Key key) && key != Key.None)
            return Keyboard.current[key].wasPressedThisFrame;

        return false;
#else
        return Input.GetKeyDown(keyCode);
#endif
    }

    private bool ReadKeyUp(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null) return false;
        string keyName = keyCode.ToString().Replace("Alpha", "Digit");
        if (System.Enum.TryParse(keyName, out Key key) && key != Key.None)
            return Keyboard.current[key].wasReleasedThisFrame;
        return false;
#else
        return Input.GetKeyUp(keyCode);
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = playerRb != null ? playerRb.position : transform.position;
        Gizmos.DrawWireSphere(pos, interactRange);
    }

#endregion

}