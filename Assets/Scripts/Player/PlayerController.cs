using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    private CharacterController controller;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;

    [Header("Input")]
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    private void Awake()
    {
        // Initialize Input System
        inputActions = new PlayerInputActions();
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
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
}