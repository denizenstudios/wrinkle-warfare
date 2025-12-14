using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;


// PHASE: PROTOTYPE
// PlayerController - Player movement calculations and design.
public class PlayerController : NetworkBehaviour
{
    PlayerManager playerManager;
    // ProtoAnimationManager animationManager;
    InputManager input;
    Vector3 moveDirection;
    Transform cameraObject;
    CharacterController characterController;
    NetworkObject networkObject;

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

    private float verticalVelocity;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        characterController = GetComponent<CharacterController>();
        playerManager = GetComponent<PlayerManager>();
        networkObject = GetComponentInParent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (Camera.main != null)
        {
            cameraObject = Camera.main.transform;
        }
        
        Debug.Log($"PlayerController - IsOwner: {IsOwner}, HasCamera: {cameraObject != null}");
    }

    public void HandleAllMovement()
    {
        // Only process movement if owner
        if (!IsOwner)
            return;

        HandleFall();
        HandleMovement();
        HandleRotation();
        if (playerManager.isInteracting)
            return;
    }

    public void HandleMovement()
    {
        // Only move if owner
        if (!IsOwner)
            return;

        if (cameraObject == null)
            return;

        moveDirection = cameraObject.forward * input.verticalInput;
        moveDirection += cameraObject.right * input.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection *= movementSpeed;

        Vector3 movement = moveDirection * Time.deltaTime;
        
        movement.y = verticalVelocity * Time.deltaTime;
        
        characterController.Move(movement);
    }

    private void HandleFall()
    {
        // Only apply gravity if owner
        if (!IsOwner)
            return;

        if (characterController == null)
            return;

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
        // Only rotate if owner
        if (!IsOwner)
            return;

        if (cameraObject == null)
            return;

        Vector3 targetDirection = Vector3.zero;

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
            verticalVelocity = 0;
        }

        moveDirection = Vector3.zero;

        // if (animationManager != null)
        // {
        //     animationManager.PlayTargetAnimation("Idle", true);
        // }
    }
}