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

    [Header("Input")]
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool swapInputPressed;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        // Subscribe to the camera swap button press
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
    }

    private void Movement()
    {
        GroundMovement();
    }

    private void GroundMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move.y = 0;
        move *= walkSpeed;
        controller.Move(move * Time.deltaTime);
    }
   
    private void InputManagement()
    {
        moveInput = inputActions.Player.Movement.ReadValue<Vector2>();
    }

    private void OnCameraSwap(InputAction.CallbackContext context)
    {
        
        if (thirdCam != null)
        {
            thirdCam.ShoulderSwap();
        }

    }
}