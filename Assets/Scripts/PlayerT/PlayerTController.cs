using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// PHASE: PROTOTYPE
// Player 2 Controller - The second attempt at Player movement calculations and design.
public class PlayerTController : MonoBehaviour
{
    PlayerTManager playerManager;
    // ProtoAnimationManager animationManager;
    InputManager input;
    Vector3 moveDirection;
    Transform cameraObject;
    CharacterController characterController;

    [Header("Falling")]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public LayerMask groundLayer;
    public float rayCastHeightOffset = 0.5f;

    [Header("Movement Flags")]
    public bool isGrounded;
    public bool isJumping;

    [Header("Movement Speed")]
    public float movementSpeed = 7f;
    public float rotationSpeed = 15f;

    [Header("Jump Variables")]
    public float jumpHeight = 2;
    public float gravityLvl = -15;
    public int maxJumpCount = 2;
    private int jumpCount = 0;

    // Vertical velocity for CharacterController
    private float verticalVelocity;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        cameraObject = Camera.main.transform;
        playerManager = GetComponent<PlayerTManager>();
        // animationManager = GetComponent<ProtoAnimationManager>();
    }

    public void HandleAllMovement()
    {
        HandleFall();
        HandleMovement();
        HandleRotation();
        if (playerManager.isInteracting)
            return;
    }

    public void HandleMovement()
    {
        moveDirection = cameraObject.forward * input.verticalInput;
        moveDirection += cameraObject.right * input.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection *= movementSpeed;

        // Apply horizontal movement
        Vector3 movement = moveDirection * Time.deltaTime;
        
        // Apply vertical velocity
        movement.y = verticalVelocity * Time.deltaTime;
        
        characterController.Move(movement);
    }

    private void HandleFall()
    {
        // CharacterController has built-in ground detection
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            // Reset vertical velocity when grounded
            verticalVelocity = -2f; // Small negative value to keep grounded
            inAirTimer = 0;
            jumpCount = 0;
        }
        else if (!isGrounded && !isJumping)
        {
            // if(!playerManager.isInteracting)
            // {
            //     animationManager.PlayTargetAnimation("JumpMid", true);
            // }

            inAirTimer = inAirTimer + Time.deltaTime;
            
            // Apply gravity
            verticalVelocity += gravityLvl * Time.deltaTime;
            
            // Optional: Add forward momentum while in air
            // Note: This is handled differently with CharacterController
            // You may want to adjust the leapingVelocity application
        }
        else
        {
            // Apply gravity when jumping
            verticalVelocity += gravityLvl * Time.deltaTime;
        }
    }

    private void HandleRotation()
    {
        // For shooting games, player should face where the camera is looking
        Vector3 targetDirection = Vector3.zero;

        // Get camera's forward direction (where you're aiming)
        targetDirection = cameraObject.forward;
        targetDirection.y = 0;
        targetDirection.Normalize();

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = playerRotation;
    }

    public void HandleJump()
    {
        // CharacterController's isGrounded is more reliable than manual raycasting
        if (isGrounded || jumpCount < maxJumpCount)
        {
            // animationManager.animator.SetBool("isJumping", true);
            // animationManager.PlayTargetAnimation("JumpStart", false);

            float jumpVelocity = Mathf.Sqrt(-2 * gravityLvl * jumpHeight);
            verticalVelocity = jumpVelocity;
            
            jumpCount++;
            isGrounded = false;
            isJumping = true;
        }
    }

    public void ResetMovement()
    {
        if (characterController != null)
        {
            // CharacterController doesn't use velocity directly
            // Just reset vertical velocity
            verticalVelocity = 0;
        }

        moveDirection = Vector3.zero;

        // if (animationManager != null)
        // {
        //     animationManager.PlayTargetAnimation("Idle", true);
        // }
    }
}