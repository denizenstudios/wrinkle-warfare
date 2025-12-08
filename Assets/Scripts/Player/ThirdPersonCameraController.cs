using UnityEngine;
using Unity.Cinemachine;

public class ThirdPersonCameraController : MonoBehaviour
{
    private CinemachineThirdPersonFollow thirdPersonFollow;
    
    [SerializeField] private float transitionSpeed = 5f; // Adjust for faster/slower transition
    
    private bool isLeftShoulder = true;
    private float targetCameraSide = 0f;

    private void Start()
    {
        if (thirdPersonFollow == null)
        {
            thirdPersonFollow = GetComponent<CinemachineThirdPersonFollow>();
        }
        
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraSide = 0; 
            targetCameraSide = 0;
        }
    }
    private void Update()
    {
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.CameraSide = Mathf.Lerp(
                thirdPersonFollow.CameraSide, 
                targetCameraSide, 
                Time.deltaTime * transitionSpeed
            );
        }
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