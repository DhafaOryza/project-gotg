using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private NPCMovement _npcMovement;

    private const string IsWalkingParam = "IsWalking";

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _npcMovement = GetComponent<NPCMovement>();

        if (_npcMovement == null || _animator == null)
        {
            Debug.LogWarning($"[NPCAnimation] NPCMovement component not found on {gameObject.name}!");
        }
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    /// <summary>
    /// Updates the animator walking parameter based on NPCMovement state.
    /// </summary>
    private void UpdateAnimationState()
    {
        if (_animator == null || _npcMovement == null) return;

        bool isWalking = _npcMovement.IsMoving;
        _animator.SetBool(IsWalkingParam, isWalking);
    }
}