using UnityEngine;
using Unity.Cinemachine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Camera Components")]
    private CinemachineThirdPersonFollow thirdPersonFollow;
    private CinemachineCamera cinemachineCamera;
    
    [Header("Shoulder Swap Settings")]
    [SerializeField] private float transitionSpeed = 5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 70f;
    
    private bool isLeftShoulder = true;
    private float targetCameraSide = 0f;
    
    // Camera rotation values
    private float rotationX = 0f;
    private float rotationY = 0f;

    private void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        
        if (thirdPersonFollow == null)
        {
            thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        }
        
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraSide = 0; 
            targetCameraSide = 0;
        }
        
        // Initialize rotation to current camera rotation
        Vector3 currentRotation = transform.eulerAngles;
        rotationY = currentRotation.y;
        rotationX = currentRotation.x;
    }
    
    private void Update()
    {
        // Smooth shoulder transition
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraSide = Mathf.Lerp(
                thirdPersonFollow.CameraSide, 
                targetCameraSide, 
                Time.deltaTime * transitionSpeed
            );
        }
    }

    public void RotateCamera(Vector2 lookInput)
    {
        // Apply mouse input to rotation
        rotationY += lookInput.x * mouseSensitivity;
        rotationX -= lookInput.y * mouseSensitivity; // Inverted for standard FPS controls
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        // Apply rotation to camera transform
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    public void ShoulderSwap()
    {
        if (thirdPersonFollow == null) return;
        
        isLeftShoulder = !isLeftShoulder;
        
        if (isLeftShoulder)
        {
            targetCameraSide = 0;
            Debug.Log("Switching to LEFT shoulder");
        }
        else
        {
            targetCameraSide = 1;
            Debug.Log("Switching to RIGHT shoulder");
        }
    }
}