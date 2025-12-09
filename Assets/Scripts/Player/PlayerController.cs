using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    private CharacterController controller;
    [SerializeField] private ThirdPersonCameraController thirdCam;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float rotationSmoothTime = 0.1f; // How smoothly player rotates to face movement direction

    [Header("Input")]
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool swapInputPressed;
    
    private float currentVelocity; // For smooth rotation

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.CameraSwap.performed += OnCameraSwap;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Player.CameraSwap.performed -= OnCameraSwap;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        InputManagement();
        Movement();
        CameraMovement();
    }

    private void Movement()
    {
        GroundMovement();
    }

    private void GroundMovement()
    {
        // Get camera's transform for relative movement
        Transform cameraTransform = thirdCam != null ? thirdCam.transform : Camera.main.transform;
        
        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Flatten the directions (remove Y component for ground movement)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Calculate final movement direction
        Vector3 move = (forward * moveInput.y + right * moveInput.x) * walkSpeed;
        
        // Move the character
        controller.Move(move * Time.deltaTime);
        
        // Rotate player to face movement direction (optional - comment out if you want strafing without rotation)
        if (move.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }

    private void CameraMovement()
    {
        if (thirdCam != null)
        {
            thirdCam.RotateCamera(lookInput);
        }
    }
   
    private void InputManagement()
    {
        moveInput = inputActions.Player.Movement.ReadValue<Vector2>();
        lookInput = inputActions.Player.Camera.ReadValue<Vector2>();
    }

    private void OnCameraSwap(InputAction.CallbackContext context)
    {
        if (thirdCam != null)
        {
            thirdCam.ShoulderSwap();
        }
    }
}