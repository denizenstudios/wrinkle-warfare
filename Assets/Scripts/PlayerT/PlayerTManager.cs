using UnityEngine.InputSystem;
using UnityEngine;

// PHASE: PROTOTYPE
// Player Manager - a script used to execute all functionality created for a player character

public class PlayerTManager : MonoBehaviour
{
    // Animator animator;
    InputManager input;
    PlayerTController playerTController;
    // [SerializeField] private DialogueUI dialogueUI;
    public CameraManager cameraManager;
    public bool isInteracting;
    // public DialogueUI DialogueUI => dialogueUI;
    // public IInteractable Interactable { get; set; }
    private void Awake()
    {
        input = GetComponent<InputManager>();
        playerTController = GetComponent<PlayerTController>();
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
        //     playerTController.ResetMovement();
        //     return;
        // }
        
        input.HandleAllInputs();
    }

    private void FixedUpdate()
    {
        playerTController.HandleAllMovement();
    }

    private void LateUpdate()
    {
        cameraManager.HandleAllCameraMovement();
    
        // isInteracting = animator.GetBool("isInteracting");
        // playerTController.isJumping = animator.GetBool("isJumping");
        // animator.SetBool("isGrounded", playerTController.isGrounded);
    }
}
