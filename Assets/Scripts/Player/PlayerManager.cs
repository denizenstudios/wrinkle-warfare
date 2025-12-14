using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Netcode;

// PHASE: PROTOTYPE
// Player Manager - a script used to execute all functionality created for a player character

public class PlayerManager : MonoBehaviour
{
    // Animator animator;
    InputManager input;
    PlayerController playerController;
    // [SerializeField] private DialogueUI dialogueUI;
    public CameraManager cameraManager;
    public bool isInteracting;
    // public DialogueUI DialogueUI => dialogueUI;
    // public IInteractable Interactable { get; set; }
    
    private NetworkObject networkObject;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        playerController = GetComponent<PlayerController>();
        
        // Get NetworkObject from parent (where it's actually attached)
        networkObject = GetComponentInParent<NetworkObject>();
        // animator = GetComponent<Animator>();
        
        // Set spawn position BEFORE network spawning happens
        if (networkObject != null && SpawnManager.Instance != null)
        {
            // Set position on the PARENT object (which has NetworkTransform)
            transform.parent.position = SpawnManager.Instance.GetNextSpawnPosition();
            transform.parent.rotation = SpawnManager.Instance.GetNextSpawnRotation();
            Debug.Log($"Player spawned at: {transform.parent.position}");
        }
    }

    private void Start()
    {
        // If this is a networked player and not the owner, disable input components
        if (networkObject != null && !networkObject.IsOwner)
        {
            input.enabled = false;
        }
    }

    private void Update()
    {
        // Only process input if this is the owner or if not networked
        if (networkObject != null && !networkObject.IsOwner)
            return;

        input.HandleAllInputs();
        playerController.HandleAllMovement();
    }

    private void LateUpdate()
    {
        // Only update camera for the owner
        if (networkObject != null && !networkObject.IsOwner)
            return;

        cameraManager.HandleAllCameraMovement();
    
        // isInteracting = animator.GetBool("isInteracting");
        // playerController.isJumping = animator.GetBool("isJumping");
        // animator.SetBool("isGrounded", playerController.isGrounded);
    }
}
