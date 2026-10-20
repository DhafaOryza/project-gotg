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

    // Setelah Stop(), klik kanan yang masih ditahan tidak langsung menggerakkan player lagi.
    // Flag ini hilang saat tombol mouse dilepas atau ada klik baru.
    private bool suppressHeldFollow;

    public bool IsMoving => hasTarget;

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

        HandleMovementInput();
        HandleSkillInput();
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
        bool pressed = ReadMouseDown();
        bool rawHeld = ReadMouseHeld();

        if (!rawHeld || pressed)
            suppressHeldFollow = false;

        bool held = holdToFollow && rawHeld && !suppressHeldFollow;

        if (!pressed && !held) return;

        if (TryGetMouseWorldX(out float x))
        {
            targetX = x;
            hasTarget = true;

            if (pressed && clickMarkerPrefab != null)
            {
                Instantiate(clickMarkerPrefab,
                    new Vector3(x, playerRb.position.y, playerRb.position.z),
                    Quaternion.identity);
            }
        }
    }

    // Tombol skill1..skill4 dipetakan berurutan ke slot 0..3 di equippedSkills.
    private void HandleSkillInput()
    {
        for (int slot = 0; slot < SkillSlotCount; slot++)
        {
            if (ReadKeyDown(GetSkillKey(slot)))
            {
                // Skill dibidik ke posisi kursor; kalau gagal dihitung, default ke kanan.
                float aimX = TryGetMouseWorldX(out float x) ? x : playerRb.position.x + 1f;
                playerEntity.TryUseSkill(slot, aimX);
            }
        }
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
    private bool TryGetMouseWorldX(out float x)
    {
        x = 0f;

        Vector2 screenPos = ReadMousePosition();
        Ray ray = cam.ScreenPointToRay(screenPos);

        // Bidang tegak lurus sumbu Z yang melewati player (bidang gerak 2D-nya).
        Plane plane = new Plane(Vector3.back, playerRb.position);

        if (plane.Raycast(ray, out float enter))
        {
            x = ray.GetPoint(enter).x;
            return true;
        }
        return false;
    }

    // Input: jalan di Input System baru maupun yang lama.
    private bool ReadMouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(1);
#endif
    }

    private bool ReadMouseHeld()
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = playerRb != null ? playerRb.position : transform.position;
        Gizmos.DrawWireSphere(pos, interactRange);
    }
}