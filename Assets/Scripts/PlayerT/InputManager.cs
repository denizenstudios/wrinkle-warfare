using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System

public class InputManager : MonoBehaviour
{
    ThirdPerControls thirdPerControls;
    // ProtoAnimationManager protoAnimationManager;
    PlayerTController playerTController;

    public Vector2 movementInput;
    public Vector2 cameraInput;
    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public bool jumpInput;

    public float cameraInputX;
    public float cameraInputY;

    public bool isUsingGamepad = false;

    private void Awake()
    {
        // protoAnimationManager = GetComponent<ProtoAnimationManager>();
        playerTController = GetComponent<PlayerTController>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
        // protoAnimationManager.UpdateAnimatorValues(0, moveAmount);
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerTController.HandleJump();
        }
    }

    private void DetectControlScheme(InputDevice device)
    {
        isUsingGamepad = device is Gamepad;
    }
}
