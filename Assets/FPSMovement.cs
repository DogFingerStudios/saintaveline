using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;       // Walking speed
    public float jumpHeight = 2f;      // Jump power

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f; // Look sensitivity
    public float maxLookAngle = 75f;    // Up/down clamp

    [Header("Physics")]
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;          
    private Transform cameraTransform; 
    private float xRotation = 0f;      

    void Start()
    {
        // Get the CharacterController
        controller = GetComponent<CharacterController>();

        // Find the Camera (child in the hierarchy)
        cameraTransform = GetComponentInChildren<Camera>().transform;

        // Lock the cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controller.Move(Vector3.down * 0.1f);
    }

    void Update()
    {
        // 1. Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        var localMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            localMoveSpeed *= 2;
        }

        // Rotate the Player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Is grounded: " + controller.isGrounded);
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            Debug.Log("Jump!");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
