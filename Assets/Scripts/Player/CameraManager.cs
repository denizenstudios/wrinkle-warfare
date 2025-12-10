using UnityEngine;

public class CameraManager : MonoBehaviour
{
    InputManager inputManager;

    public Transform targetTransform;
    public Transform cameraPivot;
    public Transform cameraTransform;
    public LayerMask collisionLayers;

    private float defaultPosition;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;

    [Header("Collision Settings")]
    public float cameraCollisionOffset = 0.2f;
    public float minimumCollisionOffset = 0.2f;
    public float cameraCollisionRadius = 0.2f;

    [Header("Camera Speeds")]
    public float cameraFollowSpeed = 0.05f;

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
    public float rightShoulderOffset = -1f;
    public float leftShoulderOffset = 1f;
    public float shoulderHeight = 0.3f;
    public float cameraDistance = -2.5f;
    private bool isLeftShoulder = false;
    [SerializeField] private float shoulderLerpSpeed = 2f;
    private float shoulderLerp = 1f;

    [Header("Zoom Settings")]
    public float zoomedDistance = -1f;
    public float zoomLerpSpeed = 0.008f;
    private float currentZoomDistance;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        cameraTransform = Camera.main.transform;
        
        Vector3 localPos = cameraTransform.localPosition;
        localPos.z = cameraDistance;
        cameraTransform.localPosition = localPos;

        defaultPosition = cameraDistance;
        currentZoomDistance = cameraDistance;
        
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

        if (useOverTheShoulder)
        {
            // Only lerp if not at target
            if (shoulderLerp < 1f)
            {
                shoulderLerp = Mathf.Clamp01(shoulderLerp + Time.deltaTime * shoulderLerpSpeed);
            }
            
            float targetShoulderOffset = isLeftShoulder ? leftShoulderOffset : rightShoulderOffset;
            float startShoulderOffset = isLeftShoulder ? rightShoulderOffset : leftShoulderOffset;
            float currentShoulderOffset = Mathf.Lerp(startShoulderOffset, targetShoulderOffset, shoulderLerp);
            cameraPivot.localPosition = new Vector3(currentShoulderOffset, shoulderHeight, 0);
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

        transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        
        cameraPivot.localRotation = Quaternion.Euler(pivotAngle, 0, 0);
    }

    private void HandleCameraCollision()
    {
        float targetZoomDistance = inputManager.zoomInput ? zoomedDistance : defaultPosition;
        
        // Smoothly ease zoom in and out
        currentZoomDistance = Mathf.Lerp(currentZoomDistance, targetZoomDistance, zoomLerpSpeed);
        
        float targetPosition = currentZoomDistance;
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

    public Vector3 GetAimDirection()
    {
        return cameraTransform.forward;
    }

    public void SwapShoulder()
    {
        isLeftShoulder = !isLeftShoulder;
        shoulderLerp = 0f;
    }

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