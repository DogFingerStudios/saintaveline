using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;       // Walking speed
    public float sprintSpeedFactor = 5f; // Sprint speed factor
    public float superSpringSpeedFactor = 10f; // Super sprint speed factor
    public float crouchSpeedFactor = 0.5f; // Crouch speed factor
    public bool isCrouching = false; // Is the player crouching?
    public float jumpHeight = 2f;      // Jump power

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f; // Look sensitivity
    public float maxLookAngle = 60f;    // Up/down clamp

    [Header("Physics")]
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;          
    private Transform cameraTransform; 
    private float xRotation = 0f;
    private float cachedXRotation;
    private float defaultHeight;
    private Vector3 defaultCenter;

    public bool IsInDrivingMode = false;

    void Start()
    {
        // Get the CharacterController
        controller = GetComponent<CharacterController>();

        // Get the default height and center
        defaultHeight = controller.height;
        defaultCenter = controller.center;

        // Find the Camera (child in the hierarchy)
        cameraTransform = GetComponentInChildren<Camera>().transform;

        // Lock the cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.Move(Vector3.down * 0.1f);
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            return;
        }

        // 1. Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Sprint speed factor
        var localMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Tab))
            {
                localMoveSpeed *= superSpringSpeedFactor;
            }
            else
#endif
            {
                localMoveSpeed *= sprintSpeedFactor;
            }
        }

        // Crouch code
        if (Input.GetKey(KeyCode.LeftControl))
        {
            localMoveSpeed *= crouchSpeedFactor;
            isCrouching = true;
            controller.height = 1f; // Adjust height for crouching
            controller.center = new Vector3(0f, 0.5f, 0f); // Adjust center for crouching
        }
        else if (isCrouching) // Only adjust if we were previously crouching
        {
            isCrouching = false;

            // Calculate the difference in height
            float heightDifference = defaultHeight - controller.height;

            // Adjust the vertical position based on the height difference
            transform.position += new Vector3(0f, heightDifference / 2f, 0f);

            // Reset height and center
            controller.height = defaultHeight;
            controller.center = defaultCenter;
        }

        // Rotate the camera up/down
        if (!IsInDrivingMode)
        {
            // Rotate the Player body left/right
            transform.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 2. Movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical   = Input.GetAxis("Vertical");   // W/S
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * localMoveSpeed * Time.deltaTime);

        // 3. Gravity & Jump
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // keep the player grounded
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
