using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public Transform firstPersonPosition; // Transform for first-person camera position
    public Transform thirdPersonPosition; // Transform for third-person camera position
    public float transitionSpeed = 5f; // Speed for smooth camera transitions
    public KeyCode toggleKey = KeyCode.Equals; // Key to toggle POV

    private Camera playerCamera; // Reference to the Camera component
    private bool isFirstPerson = true; // Tracks the current camera mode

    void Start()
    {
        // Get the Camera component under the Player
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("No Camera component found as a child of the Player!");
            return;
        }

        // Move the camera to the starting position (first-person view)
        playerCamera.transform.position = firstPersonPosition.position;
        playerCamera.transform.rotation = firstPersonPosition.rotation;
    }

    void LateUpdate()
    {
        // Toggle the camera mode when the key is pressed
        if (Input.GetKeyDown(toggleKey))
        {
            isFirstPerson = !isFirstPerson;
        }

        // Smoothly move the camera to the target position and rotation
        if (isFirstPerson)
        {
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                firstPersonPosition.position,
                Time.fixedDeltaTime * transitionSpeed
            );
            playerCamera.transform.rotation = Quaternion.Lerp(
                playerCamera.transform.rotation,
                firstPersonPosition.rotation,
                Time.fixedDeltaTime * transitionSpeed
            );
        }
        else
        {
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                thirdPersonPosition.position,
                Time.fixedDeltaTime * transitionSpeed
            );
            playerCamera.transform.rotation = Quaternion.Lerp(
                playerCamera.transform.rotation,
                thirdPersonPosition.rotation,
                Time.fixedDeltaTime * transitionSpeed
            );
        }
    }
}
