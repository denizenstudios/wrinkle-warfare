using UnityEngine.InputSystem;
using UnityEngine;

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
    private void Awake()
    {
        input = GetComponent<InputManager>();
        playerController = GetComponent<PlayerController>();
        // animator = GetComponent<Animator>();
    }


    private void Update()
    {
        // if (Keyboard.current.eKey.wasPressedThisFrame && !dialogueUI.isOpen)
        // {
        //     Interactable?.Interact(this);
        // }
        
        // if (dialogueUI.isOpen)
        // {
        //     playerController.ResetMovement();
        //     return;
        // }
        
        input.HandleAllInputs();
        playerController.HandleAllMovement();
    }

    private void LateUpdate()
    {
        cameraManager.HandleAllCameraMovement();
    
        // isInteracting = animator.GetBool("isInteracting");
        // playerController.isJumping = animator.GetBool("isJumping");
        // animator.SetBool("isGrounded", playerController.isGrounded);
    }
}
