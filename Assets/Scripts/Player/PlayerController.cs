using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Camera")]
    public Transform cameraTarget;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    
    private Rigidbody rb;
    private float horizontalRotation = 0f; // NEW: track horizontal rotation separately
    private float verticalRotation = 0f;
    
    // Input System variables
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    
    void Awake()
    {
        // Initialize Input System
        inputActions = new PlayerInputActions();
    }
    
    void OnEnable()
    {
        inputActions.Player.Enable();
    }
    
    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    void Start()
    {
        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody
        rb.freezeRotation = true;
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Read input values
        moveInput = inputActions.Player.Movement.ReadValue<Vector2>();
        lookInput = inputActions.Player.Camera.ReadValue<Vector2>();
        
        HandleCameraRotation();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleMovement()
    {
        // Calculate movement direction relative to camera's horizontal rotation
        float yaw = horizontalRotation * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
        Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw));
        
        // Calculate desired movement direction
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        
        // Move the player
        if (moveDirection.magnitude > 0.1f)
        {
            // Move
            Vector3 targetVelocity = moveDirection * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
            
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    void HandleCameraRotation()
    {
        // Get mouse delta
        float mouseX = lookInput.x * mouseSensitivity * 0.02f;
        float mouseY = lookInput.y * mouseSensitivity * 0.02f;
        
        // Update horizontal rotation
        horizontalRotation += mouseX;
        
        // Update vertical rotation with limits
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        // Apply rotation to camera target (not affected by player body rotation)
        cameraTarget.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}