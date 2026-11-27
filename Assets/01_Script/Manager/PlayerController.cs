using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string controllerBindingGroup = "Controller";
    [SerializeField] private string keyboardBindingGroup = "Keyboard";

    [Header("Movement")]
    [SerializeField] private float stopDistance = 0.1f;

    [SerializeField, Range(0f, 1f)] private float movementDeadzone = 0.15f;
    [SerializeField] private bool negativeScaleFacesRight = true;

    [Header("Ghost Preview & Snapping Settings")]
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

    [Header("Interact Key")]
    public KeyCode interact = KeyCode.F;

    [Header("Interact Settings")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactLayers = ~0;

    private const int SkillSlotCount = 4;

    private InputActionAsset runtimeInputActions;
    private InputActionMap playerActionMap;
    private string activeBindingGroup;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction cancelAction;
    private InputAction virtualCursorAction;
    private readonly InputAction[] skillActions = new InputAction[SkillSlotCount];
    private readonly InputAction[] itemActions = new InputAction[SkillSlotCount];


    private PlayerBaseEntity playerEntity;
    private GameObject playerObject;
    private Rigidbody playerRb;

    private float targetX;
    private bool hasTarget;
    private float facingDirection = 1f;
    private bool gameplayInputEnabled = true;

    // Buffer untuk OverlapSphereNonAlloc supaya tidak membuat array baru tiap interact.
    private readonly Collider[] interactBuffer = new Collider[16];

    //Tracking Mode
    private bool isAiming = false;
    private int activeAimSlot = -1;
    private Vector2 currentAimDirection = Vector2.right;
    private Vector2 _mouseWorldPosition;

    public bool IsMoving => hasTarget;
    public bool IsAiming => isAiming;
    public Vector2 CurrentAimDirection => currentAimDirection;
    public InputAction VirtualCursorAction => virtualCursorAction;
    public InputActionAsset RuntimeInputActions => runtimeInputActions;
    public bool IsUsingController => activeBindingGroup == controllerBindingGroup;

    private void OnEnable()
    {
        InputSystem.onDeviceChange += HandleDeviceChange;
        KeybindManager.OnOverridesChanged += ApplyKeybindOverrides;
        InitializeInputActions();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
        KeybindManager.OnOverridesChanged -= ApplyKeybindOverrides;

        CancelAiming();

        if (playerActionMap != null)
        {
            if (moveAction != null) moveAction.Disable();
            if (interactAction != null) interactAction.performed -= HandleInteract;
            if (cancelAction != null) cancelAction.performed -= HandleCancel;
            if (virtualCursorAction != null) virtualCursorAction.Disable();

            foreach (var action in skillActions)
            {
                if (action == null) continue;
                action.started -= HandleSkillStarted;
                action.canceled -= HandleSkillCanceled;
            }

            foreach (var action in itemActions)
            {
                if (action != null) action.performed -= HandleItemPerformed;
            }

            playerActionMap.Disable();
        }

        if (runtimeInputActions != null)
            Destroy(runtimeInputActions);

        runtimeInputActions = null;
        playerActionMap = null;
        activeBindingGroup = null;
        moveAction = null;
        interactAction = null;
        cancelAction = null;
        virtualCursorAction = null;
        System.Array.Clear(skillActions, 0, skillActions.Length);
        System.Array.Clear(itemActions, 0, itemActions.Length);
    }

    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("[PlayerController] Input Actions asset belum di-assign.", this);
            return;
        }

        runtimeInputActions = Instantiate(inputActions);
        playerActionMap = runtimeInputActions.FindActionMap(actionMapName, false);

        if (playerActionMap == null)
        {
            Debug.LogError($"[PlayerController] Action Map '{actionMapName}' tidak ditemukan.", this);
            Destroy(runtimeInputActions);
            runtimeInputActions = null;
            return;
        }

        moveAction = playerActionMap.FindAction("Move", false);
        interactAction = playerActionMap.FindAction("Interact", false);
        cancelAction = playerActionMap.FindAction("Cancel", false);
        virtualCursorAction = runtimeInputActions.FindAction("Virtual Cursor", false);


        //Virtual Cursor
        if (virtualCursorAction == null)
            virtualCursorAction = runtimeInputActions.FindAction("Virtual Cursor", false);

        if (virtualCursorAction != null)
        {
            virtualCursorAction.Enable();
            Debug.Log("<color=green>[PlayerController] Sukses menemukan dan mengaktifkan Action 'Virtual Cursor'!</color>");
        }
        else
        {
            Debug.LogError("[PlayerController] GAGAL: Action 'Virtual Cursor' tidak ditemukan di InputActionAsset!");
        }
        //
        
        if (interactAction != null)
            interactAction.performed += HandleInteract;

        if (cancelAction != null)
            cancelAction.performed += HandleCancel;

        for (int slot = 0; slot < SkillSlotCount; slot++)
        {
            skillActions[slot] = playerActionMap.FindAction($"Skill {slot + 1}", false);
            if (skillActions[slot] != null)
            {
                skillActions[slot].started += HandleSkillStarted;
                skillActions[slot].canceled += HandleSkillCanceled;
            }

            itemActions[slot] = playerActionMap.FindAction($"Item {slot + 1}", false);
            if (itemActions[slot] != null)
                itemActions[slot].performed += HandleItemPerformed;
        }

        UpdateBindingGroupForConnectedDevices();
        ApplyKeybindOverrides();

        playerActionMap.Enable();
        if (!gameplayInputEnabled)
            playerActionMap.Disable();
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        if (gameplayInputEnabled == enabled) return;

        gameplayInputEnabled = enabled;
        if (!enabled)
        {
            hasTarget = false;
            CancelAiming();
        }

        if (playerActionMap == null || !isActiveAndEnabled) return;

        if (enabled)
            playerActionMap.Enable();
        else
            playerActionMap.Disable();
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            SetBindingGroup(controllerBindingGroup);
            return;
        }

        if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected ||
            change == InputDeviceChange.Disabled)
        {
            UpdateBindingGroupForConnectedDevices(device);
            return;
        }

        UpdateBindingGroupForConnectedDevices();
    }

    private void UpdateBindingGroupForConnectedDevices(InputDevice unavailableDevice = null)
    {
        bool hasGamepad = false;
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad != unavailableDevice && gamepad.added && gamepad.enabled)
            {
                hasGamepad = true;
                break;
            }
        }

        SetBindingGroup(hasGamepad ? controllerBindingGroup : keyboardBindingGroup);
    }

    private void SetBindingGroup(string bindingGroup)
    {
        if (runtimeInputActions == null || playerActionMap == null || activeBindingGroup == bindingGroup)
            return;

        CancelAiming();
        bool mapWasEnabled = playerActionMap.enabled;
        if (mapWasEnabled)
            playerActionMap.Disable();

        runtimeInputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);
        activeBindingGroup = bindingGroup;

        if (mapWasEnabled)
            playerActionMap.Enable();

        Debug.Log($"[PlayerController] Input scheme aktif: {bindingGroup}", this);
    }

    public string GetInteractDisplayName()
    {
        return interactAction != null ? interactAction.GetBindingDisplayString() : interact.ToString();
    }

    public string GetSkillDisplayName(int slot)
    {
        if (slot < 0 || slot >= SkillSlotCount) return string.Empty;
        return skillActions[slot] != null ? skillActions[slot].GetBindingDisplayString() : GetSkillKey(slot).ToString();
    }

    public string GetItemDisplayName(int slot)
    {
        if (slot < 0 || slot >= SkillSlotCount) return string.Empty;
        return itemActions[slot] != null ? itemActions[slot].GetBindingDisplayString() : $"Item {slot + 1}";
    }

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
        facingDirection = playerObject.transform.localScale.x < 0f == negativeScaleFacesRight ? 1f : -1f;

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
        if (playerEntity == null || playerRb == null) return;

        // Player yang knockdown atau mati tidak boleh bergerak maupun memakai skill.
        if (!playerEntity.CanAct)
        {
            hasTarget = false;
            if (isAiming) CancelAiming();
            return;
        }

        if (isAiming)
            CalculateAimDirection();
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

    private void HandleInteract(InputAction.CallbackContext context)
    {
        if (playerEntity != null && playerEntity.CanAct)
            TryInteract();
    }

    private void HandleCancel(InputAction.CallbackContext context)
    {
        if (isAiming)
            CancelAiming();
    }

    private void HandleSkillStarted(InputAction.CallbackContext context)
    {
        if (playerEntity == null || !playerEntity.CanAct || playerEntity.HasPendingSkill) return;

        int slot = GetActionSlot(context.action.name, "Skill ");
        if (slot < 0 || slot >= SkillSlotCount) return;

        if (playerEntity.GetSkillCooldownRemaining(slot) > 0f)
        {
            Debug.Log($"[Skill] slot {slot} masih cooldown");
            return;
        }

        var skillData = playerEntity.GetEquippedSkillData(slot);
        if (skillData == null) return;

        if (skillData.targetType == SkillTargetType.Self)
            QueueSkillForAttack(slot, GetFacingAimDirection());
        else
            StartAiming(slot);
    }

    private void HandleSkillCanceled(InputAction.CallbackContext context)
    {
        if (!isAiming || playerEntity == null ||
            activeAimSlot != GetActionSlot(context.action.name, "Skill ") || playerEntity.HasPendingSkill)
            return;

        if (!playerEntity.CanAct)
        {
            CancelAiming();
            return;
        }

        CalculateAimDirection();
        ExecuteActiveSkill();
    }

    private void HandleItemPerformed(InputAction.CallbackContext context)
    {
        if (playerEntity == null || !playerEntity.CanAct) return;

        int slot = GetActionSlot(context.action.name, "Item ");
        if (slot < 0 || slot >= SkillSlotCount) return;

        GameManager.Instance?.itemManager?.UseItem(slot, playerEntity);
    }

    private static int GetActionSlot(string actionName, string prefix)
    {
        if (!actionName.StartsWith(prefix) || !int.TryParse(actionName.Substring(prefix.Length), out int oneBasedSlot))
            return -1;

        return oneBasedSlot - 1;
    }

#region  Aiming System

    private void StartAiming(int slotIndex)
    {
        Stop();
        
        isAiming = true;
        activeAimSlot = slotIndex;
        
        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Hitung arah awal langsung menggunakan raycast mouse
        CalculateAimDirection();
        
        var skillData = playerEntity.GetEquippedSkillData(slotIndex);
        if (skillData != null)
            SpawnGhost(skillData);
    }

    private void CalculateAimDirection()
    {
        if (playerEntity == null || playerRb == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 origin = attackPoint != null ? attackPoint.position : playerRb.position;

        // Ambil posisi mouse di layar
        Vector2 screenMousePos = Vector2.zero;
        if (UnityEngine.InputSystem.Mouse.current != null)
            screenMousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        else
            screenMousePos = Input.mousePosition;

        // Proyeksikan ke world plane Z karakter
        Plane playerPlane = new Plane(Vector3.forward, new Vector3(0, 0, origin.z));
        Ray ray = mainCamera.ScreenPointToRay(screenMousePos);

        if (playerPlane.Raycast(ray, out float enter))
        {
            _mouseWorldPosition = ray.GetPoint(enter);
        }
        else
        {
            float distanceToPlane = Mathf.Abs(mainCamera.transform.position.z - origin.z);
            Vector3 mouseWithZ = new Vector3(screenMousePos.x, screenMousePos.y, distanceToPlane);
            _mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseWithZ);
        }
        
        // Ambil titik batas bawah (misal posisi kaki / pivot playerRb)
        float groundLimitY = playerRb.position.y; 

        // OPSI A: Jika mouse berada di bawah karakter, kunci Y-nya agar minimal horizontal (0)
        Vector2 rawDirection = (Vector2)_mouseWorldPosition - (Vector2)origin;
        
        // Jika arah mengarah ke bawah, paksa Y tidak boleh negatif
        if (rawDirection.y < 0f)
        {
            rawDirection.y = 0f; 
        }

        Vector2 directionToMouse = rawDirection.normalized;

        // Jika mouse tepat di tengah sehingga arahnya Vector2.zero, gunakan arah hadap
        if (directionToMouse.sqrMagnitude < 0.001f)
        {
            directionToMouse = GetFacingAimDirection();
        }

        currentAimDirection = directionToMouse;
        SetFacingDirection(currentAimDirection.x);

        // Update posisi AttackPoint (X flip)
        if (attackPoint != null && attackPoint != playerObject.transform)
        {
            Vector3 localPos = attackPoint.localPosition;
            float absX = Mathf.Abs(localPos.x);
            float scaleDirection = Mathf.Sign(playerObject.transform.lossyScale.x);
            localPos.x = facingDirection * scaleDirection >= 0f ? absX : -absX;
            attackPoint.localPosition = localPos;
        }

        // Hitung sudut rotasi Ghost Preview (karena Y sudah dikunci >= 0, angle akan antara 0° s/d 180°)
        float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;

        if (currentGhost != null)
        {
            float ghostDistance = 1.5f;
            Vector3 ghostPosition = origin + (Vector3)(currentAimDirection * ghostDistance);
            ghostPosition.z = origin.z;
            Quaternion ghostRotation = Quaternion.Euler(0f, 0f, angle);
            currentGhost.SetPositionandRotation(ghostPosition, ghostRotation);
        }
    }

    private Vector2 GetFacingAimDirection()
    {
        return facingDirection >= 0f ? Vector2.right : Vector2.left;
    }

    private void SetFacingDirection(float directionX)
    {
        if (Mathf.Abs(directionX) <= movementDeadzone) return;

        facingDirection = Mathf.Sign(directionX);
        if (playerObject == null) return;

        Vector3 scale = playerObject.transform.localScale;
        bool shouldBeNegative = (facingDirection > 0f) == negativeScaleFacesRight;
        scale.x = Mathf.Abs(scale.x) * (shouldBeNegative ? -1f : 1f);
        playerObject.transform.localScale = scale;
    }

    private void ExecuteActiveSkill()
    {
        int slot = activeAimSlot;
        Vector2 direction = currentAimDirection;
        DestroyGhost();
        ResetTimeScale();
        isAiming = false;
        activeAimSlot = -1;
        QueueSkillForAttack(slot, direction);
    }

    private void QueueSkillForAttack(int slot, Vector2 direction)
    {
        if (playerEntity == null || !playerEntity.CanAct) return;

        playerEntity.RequestSkillWithAnimation(slot, attackPoint, direction);
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

#region Keybind

    private void ApplyKeybindOverrides()
    {
        if (runtimeInputActions == null) return;
    
        var mapWasEnabled = playerActionMap != null && playerActionMap.enabled;
        if (mapWasEnabled)
            playerActionMap.Disable();
    
        foreach (var map in runtimeInputActions.actionMaps)
        {
            foreach (var action in map.actions)
                action.RemoveAllBindingOverrides();
        }
    
        CopyOverridesFrom(inputActions);
    
        var applied = 0;
        if (KeybindManager.Instance != null)
        {
            foreach (var kvp in KeybindManager.Instance.GetAllOverrides())
            {
                if (!TryParseOverrideKey(kvp.Key, out var actionName, out var partName, out var schemeName))
                    continue;
                if (string.IsNullOrEmpty(kvp.Value)) continue;
    
                var action = runtimeInputActions.FindAction(actionName, false);
                if (action == null) continue;
    
                var bindingIndex = FindOverrideBindingIndex(action, schemeName, partName);
                if (bindingIndex < 0) continue;
    
                action.ApplyBindingOverride(bindingIndex, kvp.Value);
                applied++;
            }
        }
    
        if (mapWasEnabled)
            playerActionMap.Enable();
    
        Debug.Log($"[PlayerController] Keybind override aktif: {applied}", this);
    }
    
    private void CopyOverridesFrom(InputActionAsset source)
    {
        if (source == null || source == runtimeInputActions) return;
    
        foreach (var sourceMap in source.actionMaps)
        {
            var destMap = runtimeInputActions.FindActionMap(sourceMap.name, false);
            if (destMap == null) continue;
    
            foreach (var sourceAction in sourceMap.actions)
            {
                var destAction = destMap.FindAction(sourceAction.name, false);
                if (destAction == null) continue;
    
                var count = Mathf.Min(sourceAction.bindings.Count, destAction.bindings.Count);
                for (var i = 0; i < count; i++)
                {
                    var overridePath = sourceAction.bindings[i].overridePath;
                    if (string.IsNullOrEmpty(overridePath)) continue;
                    destAction.ApplyBindingOverride(i, overridePath);
                }
            }
        }
    }
    
    private static bool TryParseOverrideKey(string key, out string actionName, out string partName, out string schemeName)
    {
        actionName = null;
        partName = null;
        schemeName = null;
        if (string.IsNullOrEmpty(key)) return false;
    
        var schemeSplit = key.LastIndexOf('_');
        if (schemeSplit <= 0 || schemeSplit >= key.Length - 1) return false;
    
        schemeName = key.Substring(schemeSplit + 1);
        var body = key.Substring(0, schemeSplit);
        var partSplit = body.IndexOf('|');
        if (partSplit >= 0)
        {
            actionName = body.Substring(0, partSplit);
            partName = body.Substring(partSplit + 1);
        }
        else
        {
            actionName = body;
        }
    
        return !string.IsNullOrEmpty(actionName);
    }
    
    private int FindOverrideBindingIndex(InputAction action, string schemeName, string partName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isComposite) continue;
    
            if (string.IsNullOrEmpty(partName))
            {
                if (binding.isPartOfComposite) continue;
            }
            else if (!binding.isPartOfComposite || binding.name != partName)
            {
                continue;
            }
    
            if (BindingMatchesScheme(action, i, schemeName))
                return i;
        }
    
        return -1;
    }
    
    private bool BindingMatchesScheme(InputAction action, int bindingIndex, string schemeName)
    {
        var groups = action.bindings[bindingIndex].groups;
        if (string.IsNullOrEmpty(groups))
        {
            for (int i = bindingIndex; i >= 0; i--)
            {
                if (!action.bindings[i].isComposite) continue;
                groups = action.bindings[i].groups;
                break;
            }
        }
    
        if (string.IsNullOrEmpty(groups)) return false;
        if (groups.Contains(schemeName)) return true;
        if (schemeName == controllerBindingGroup && groups.Contains("Gamepad")) return true;
        if (schemeName == keyboardBindingGroup && groups.Contains("Keyboard")) return true;
        return false;
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

        if (playerEntity.IsKnockedBack) // knockback
        {
            hasTarget = false;
            return;
        }

        Vector3 vel = playerRb.linearVelocity; // Unity 6. Di versi lama pakai .velocity

        if (!playerEntity.CanAct)
        {
            hasTarget = false;
            vel.x = 0f;
            playerRb.linearVelocity = vel;
            return;
        }

        Vector2 movement = isAiming || moveAction == null ? Vector2.zero : moveAction.ReadValue<Vector2>();
        if (Mathf.Abs(movement.x) > movementDeadzone)
        {
            hasTarget = false;
            SetFacingDirection(movement.x);
            vel.x = movement.x * playerEntity.MoveSpeed;
            playerRb.linearVelocity = vel;
            return;
        }

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
            SetFacingDirection(delta);
            var moveSpeed = playerEntity.MoveSpeed;
            vel.x = Mathf.Sign(delta) * moveSpeed;
        }

        // Y tetap dibiarkan supaya gravitasi dan lompat tidak terganggu.
        playerRb.linearVelocity = vel;
    }

    // Hentikan pergerakan seketika: hapus target dan nolkan kecepatan horizontal sekarang juga.
    public void Stop()
    {
        hasTarget = false;
        if (isAiming) CancelAiming();

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = playerRb != null ? playerRb.position : transform.position;
        Gizmos.DrawWireSphere(pos, interactRange);
    }

}