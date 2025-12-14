using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class InputManager : NetworkBehaviour
{
    ThirdPerControls thirdPerControls;
    // ProtoAnimationManager protoAnimationManager;
    PlayerController playerController;
    CameraManager cameraManager;
    NetworkObject networkObject;

    public Vector2 movementInput;
    public Vector2 cameraInput;
    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public bool jumpInput;
    public bool zoomInput;

    public float cameraInputX;
    public float cameraInputY;

    public bool isUsingGamepad = false;

    private void Awake()
    {
        // protoAnimationManager = GetComponent<ProtoAnimationManager>();
        playerController = GetComponent<PlayerController>();
        cameraManager = FindObjectOfType<CameraManager>();
        
        // Get NetworkObject from parent (where it's actually attached)
        networkObject = GetComponentInParent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Only initialize input for the owner
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log($"InputManager - Owner spawned");
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
        if (thirdPerControls == null)
        {
            thirdPerControls = new ThirdPerControls();

            thirdPerControls.PlayerMovement.Movement.performed += ctx =>
            {
                movementInput = ctx.ReadValue<Vector2>();
                DetectControlScheme(ctx.control.device);
            };

            thirdPerControls.PlayerMovement.Camera.performed += ctx =>
            {
                cameraInput = ctx.ReadValue<Vector2>();
                DetectControlScheme(ctx.control.device);
            };

            thirdPerControls.PlayerActions.Jump.performed += ctx =>
            {
                jumpInput = true;
                DetectControlScheme(ctx.control.device);
            };
        }

        thirdPerControls.Enable();
    }

    private void Update()
    {
        // Make sure input system is initialized
        if (thirdPerControls == null)
        {
            thirdPerControls = new ThirdPerControls();
            thirdPerControls.Enable();
        }

        // ALWAYS read input values - don't check ownership here
        // PlayerManager will handle which player processes it
        if (thirdPerControls != null)
        {
            movementInput = thirdPerControls.PlayerMovement.Movement.ReadValue<Vector2>();
            cameraInput = thirdPerControls.PlayerMovement.Camera.ReadValue<Vector2>();
            
            if (movementInput.magnitude > 0.1f)
            {
                Debug.Log($"[InputManager] Raw movementInput: {movementInput}");
            }
        }
        
        // Check for right mouse button zoom
        zoomInput = Mouse.current != null && Mouse.current.rightButton.isPressed;
        
        // Check for camera swap input (X key)
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            // Only swap shoulder if owner
            if (networkObject != null && !networkObject.IsOwner)
                return;
                
            cameraManager.SwapShoulder();
        }
    }


    private void OnDisable()
    {
        thirdPerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleJumpInput();
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        
        if (moveAmount > 0)
        {
            Debug.Log($"Movement Input - H: {horizontalInput}, V: {verticalInput}, Amount: {moveAmount}");
        }
        // protoAnimationManager.UpdateAnimatorValues(0, moveAmount);
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerController.HandleJump();
        }
    }

    private void DetectControlScheme(InputDevice device)
    {
        isUsingGamepad = device is Gamepad;
    }
}

