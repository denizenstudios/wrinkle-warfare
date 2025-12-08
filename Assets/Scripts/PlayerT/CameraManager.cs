using UnityEngine;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;

    public Transform targetTransform;   // The object the camera follows
    public Transform cameraPivot;       // The object the camera pivots around
    public Transform cameraTransform;   // The camera object
    public LayerMask collisionLayers;

    private float defaultPosition;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;

    [Header("Collision Settings")]
    public float cameraCollisionOffset = 0.2f;
    public float minimumCollisionOffset = 0.2f;
    public float cameraCollisionRadius = 0.2f;

    [Header("Camera Speeds")]
    public float cameraFollowSpeed = 0.1f;

    [Header("Sensitivity")]
    public float mouseSensitivity = 2f;
    public float controllerSensitivity = 100f;

    [Header("Rotation Settings")]
    public float lookAngle;    // Y-axis rotation
    public float pivotAngle;   // X-axis rotation
    public float minPivotAngle = -65f;
    public float maxPivotAngle = 45f;

    [Header("Over-The-Shoulder Settings")]
    public bool useOverTheShoulder = true;
    public float shoulderOffset = -1.5f;  // How far to offset the camera to the right
    public float shoulderHeight = 0.75f;  // Height offset for the pivot
    public float cameraDistance = -3f;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        cameraTransform = Camera.main.transform;
        
        // Set the initial camera position with all offsets
        Vector3 localPos = cameraTransform.localPosition;
        localPos.z = cameraDistance;
        cameraTransform.localPosition = localPos;
        
        // NOW capture the default position after setting it
        defaultPosition = cameraDistance;  // Use the variable, not the current position
        
        // Lock and hide cursor for better aiming experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleAllCameraMovement();
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollision();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp(
            transform.position,
            targetTransform.position,
            ref cameraFollowVelocity,
            cameraFollowSpeed
        );

        transform.position = targetPosition;

        // Apply over-the-shoulder offset
        if (useOverTheShoulder)
        {
            // Offset the pivot to the side and up
            cameraPivot.localPosition = new Vector3(shoulderOffset, shoulderHeight, 0);
        }
        else
        {
            cameraPivot.localPosition = Vector3.zero;
        }
    }

    private void RotateCamera()
    {
        float sensitivity = inputManager.isUsingGamepad ? controllerSensitivity * Time.deltaTime : mouseSensitivity;

        // Camera rotation drives the aim
        lookAngle += inputManager.cameraInputX * sensitivity;
        pivotAngle -= inputManager.cameraInputY * sensitivity;

        pivotAngle = Mathf.Clamp(pivotAngle, minPivotAngle, maxPivotAngle);

        // Rotate the camera rig (horizontal rotation)
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        
        // Rotate the pivot (vertical rotation)
        cameraPivot.localRotation = Quaternion.Euler(pivotAngle, 0, 0);
    }

    private void HandleCameraCollision()
    {
        float targetPosition = defaultPosition;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition), collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition = -(distance - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = targetPosition - minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;
    }

    // Get the direction the camera is facing for aiming
    public Vector3 GetAimDirection()
    {
        return cameraTransform.forward;
    }

    // Get the point where the reticle is aiming in world space
    public Vector3 GetAimPoint(float maxDistance = 100f)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            return hit.point;
        }

        return ray.GetPoint(maxDistance);
    }
}