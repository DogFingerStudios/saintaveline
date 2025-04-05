using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enhanced FPS movement script with camera collision prevention and NPC stepping prevention.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnhancedFPSMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;         // Walking speed
    public float sprintSpeedFactor = 2.0f; // Sprint speed factor
    public float jumpHeight = 2.0f;      // Jump power

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;  // Look sensitivity
    public float maxLookAngle = 75f;     // Up/down clamp

    [Header("Physics")]
    public float gravity = -40f;         // Increased for less floaty feel

    [Header("Camera Collision Prevention")]
    public bool preventCameraClipping = true;
    public float clipDistance = 0.2f;
    public float minNearClipPlane = 0.005f;
    public float defaultNearClipPlane = 0.03f;
    public int sampleCount = 30;
    public LayerMask cameraCollisionLayers;

    [Header("NPC Collision Prevention")]
    public bool preventNPCStepping = true;
    public LayerMask npcLayerMask;
    public float npcDetectionRadius = 1.5f;
    public float npcExtraHeight = 0.3f;
    public float npcUpdateFrequency = 0.1f;
    public bool showNPCDebugVisuals = true;

    // Private variables for movement
    private CharacterController controller;
    private Vector3 velocity;
    private Transform cameraTransform;
    private float xRotation = 0f;
    private float fallTimeCounter = 0f;
    private Camera playerCamera;

    // Private variables for camera collision
    private float originalNearClipPlane;
    private Vector3[] checkDirections;
    private bool isColliding = false;
    private float currentDistance = 0f;

    // Private variables for NPC collision
    private List<NPCCollider> npcColliders = new List<NPCCollider>();
    private float npcUpdateTimer = 0f;
    
    // Debug visualization storage
    private List<Vector3> debugCubePositions = new List<Vector3>();
    private List<Vector3> debugCubeSizes = new List<Vector3>();

    // Class to store NPC collider information
    private class NPCCollider
    {
        public Transform npcTransform;
        public GameObject tempCollider;
        public float lastDistance;

        public NPCCollider(Transform transform)
        {
            npcTransform = transform;
            tempCollider = null;
            lastDistance = float.MaxValue;
        }
    }

    void Start()
    {
        // Get components
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (playerCamera == null)
        {
            Debug.LogError("No camera found as a child of the player!");
            enabled = false;
            return;
        }
        
        cameraTransform = playerCamera.transform;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize camera collision prevention
        if (preventCameraClipping)
        {
            originalNearClipPlane = playerCamera.nearClipPlane;
            GenerateCheckDirections();
        }
        
        // Initial grounding
        controller.Move(Vector3.down * 0.1f);
    }

    // Update has been replaced with LateUpdate (see below)

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the Player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        var localMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            localMoveSpeed *= sprintSpeedFactor;
        }

        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");   // W/S
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * localMoveSpeed * Time.deltaTime);
    }

    void HandleGravityAndJump()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            // Just landed from a fall
            if (fallTimeCounter > 0.1f)
            {
                Debug.Log("Landed after falling for " + fallTimeCounter + " seconds");
                fallTimeCounter = 0f;
            }
            
            velocity.y = -2f; // keep the player grounded
        }
        else
        {
            // Track fall time
            fallTimeCounter += Time.deltaTime;
            
            // Apply basic gravity
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply vertical movement
        controller.Move(velocity * Time.deltaTime);

        // Debug controls
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Is grounded: " + controller.isGrounded + ", Y velocity: " + velocity.y);
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            Debug.Log("Jump!");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            fallTimeCounter = 0f;
        }
    }

    #region Camera Collision Prevention

    // Generate directions for spherical checks using the Fibonacci sphere algorithm
    private void GenerateCheckDirections()
    {
        checkDirections = new Vector3[sampleCount];
        
        // Add the six cardinal directions first to ensure they're always checked
        Vector3[] cardinalDirections = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            Vector3.up,
            Vector3.down
        };
        
        for (int i = 0; i < cardinalDirections.Length && i < sampleCount; i++)
        {
            checkDirections[i] = cardinalDirections[i];
        }
        
        // Fill the rest with points on a Fibonacci sphere for better distribution
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        for (int i = cardinalDirections.Length; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = 2 * Mathf.PI * goldenRatio * i;
            
            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            
            checkDirections[i] = new Vector3(x, y, z).normalized;
        }
    }

    private void HandleCameraCollision()
    {
        // Reset collision state
        isColliding = false;
        currentDistance = float.MaxValue;
        
        // First do a sphere cast to catch any close collisions
        if (Physics.CheckSphere(cameraTransform.position, clipDistance, cameraCollisionLayers, QueryTriggerInteraction.Ignore))
        {
            // If we're inside a collider, use a very small near clip plane
            playerCamera.nearClipPlane = minNearClipPlane;
            return;
        }
        
        // Check for collisions from multiple directions
        CheckCameraCollisions();
        
        // Adjust near clip plane based on collision results
        if (isColliding)
        {
            // Calculate new near clip plane value based on collision distance
            // The closer the collision, the smaller the near clip plane needs to be
            float adjustedClipPlane = Mathf.Lerp(minNearClipPlane, defaultNearClipPlane, currentDistance / clipDistance);
            playerCamera.nearClipPlane = adjustedClipPlane;
        }
        else
        {
            // Restore default near clip plane when not colliding
            playerCamera.nearClipPlane = defaultNearClipPlane;
        }
    }

    private void CheckCameraCollisions()
    {
        // Cast rays in all pre-computed directions
        foreach (Vector3 direction in checkDirections)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.TransformDirection(direction), out RaycastHit hit, clipDistance, cameraCollisionLayers, QueryTriggerInteraction.Ignore))
            {
                // We hit something - record collision
                isColliding = true;
                
                // Keep track of the closest collision
                if (hit.distance < currentDistance)
                {
                    currentDistance = hit.distance;
                }
                
                // Special ceiling detection
                Vector3 hitNormal = hit.normal;
                // If the hit normal is pointing downward, it's likely a ceiling
                if (hitNormal.y < -0.7f)
                {
                    // Draw ceiling hits in a distinctive color
                    Debug.DrawLine(cameraTransform.position, hit.point, Color.magenta, 0.1f);
                }
                else
                {
                    // Draw other hits
                    Debug.DrawLine(cameraTransform.position, hit.point, Color.red, 0.1f);
                }
            }
            else
            {
                // Draw debug ray for rays that don't hit anything
                Debug.DrawRay(cameraTransform.position, cameraTransform.TransformDirection(direction) * clipDistance, Color.green, 0.1f);
            }
        }
    }

    #endregion

    #region NPC Collision Prevention

    private void HandleNPCCollision()
    {
        // Update timer
        npcUpdateTimer += Time.deltaTime;
        
        // Update NPC tracking on frequency
        if (npcUpdateTimer >= npcUpdateFrequency)
        {
            FindNearbyNPCs();
            npcUpdateTimer = 0f;
        }
        
        // Update collision handling every frame
        UpdateNPCColliders();
    }

    private void FindNearbyNPCs()
    {
        // Find all NPCs within detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, npcDetectionRadius, npcLayerMask);
        
        // Clean up list - remove NPCs no longer in range
        for (int i = npcColliders.Count - 1; i >= 0; i--)
        {
            bool found = false;
            foreach (Collider col in nearbyColliders)
            {
                if (col.transform == npcColliders[i].npcTransform)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                // NPC no longer in range, destroy temp collider
                if (npcColliders[i].tempCollider != null)
                {
                    Destroy(npcColliders[i].tempCollider);
                }
                npcColliders.RemoveAt(i);
            }
        }
        
        // Add new NPCs to list
        foreach (Collider col in nearbyColliders)
        {
            bool found = false;
            foreach (NPCCollider npc in npcColliders)
            {
                if (npc.npcTransform == col.transform)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                // New NPC found, add to list
                npcColliders.Add(new NPCCollider(col.transform));
            }
        }
    }

    private void UpdateNPCColliders()
    {
        foreach (NPCCollider npc in npcColliders)
        {
            if (npc.npcTransform == null)
                continue;
                
            // Calculate distance to this NPC
            float distance = Vector3.Distance(transform.position, npc.npcTransform.position);
            npc.lastDistance = distance;
            
            // Get NPC collider information
            Collider npcCollider = npc.npcTransform.GetComponent<Collider>();
            if (npcCollider == null)
                continue;
                
            // Create or update temporary collision volume
            if (npc.tempCollider == null)
            {
                // Create a new game object with a box collider
                npc.tempCollider = new GameObject("NPC_HeadBlocker_" + npc.npcTransform.name);
                npc.tempCollider.layer = LayerMask.NameToLayer("Default"); // Set to appropriate layer
                
                BoxCollider boxCollider = npc.tempCollider.AddComponent<BoxCollider>();
                boxCollider.isTrigger = false; // Make it a solid collider
            }
            
            // Update the collider position and size
            UpdateColliderSize(npc, npcCollider);
        }
    }

    private void UpdateColliderSize(NPCCollider npc, Collider npcCollider)
    {
        if (npc.tempCollider == null)
            return;
            
        BoxCollider boxCollider = npc.tempCollider.GetComponent<BoxCollider>();
        if (boxCollider == null)
            return;
            
        // Get the NPC's size
        Vector3 size = Vector3.zero;
        Vector3 center = Vector3.zero;
        
        if (npcCollider is CapsuleCollider capsule)
        {
            // For a capsule collider
            size = new Vector3(
                capsule.radius * 2,
                capsule.height,
                capsule.radius * 2
            );
            center = capsule.center;
        }
        else if (npcCollider is BoxCollider box)
        {
            // For a box collider
            size = box.size;
            center = box.center;
        }
        else
        {
            // For other collider types, use bounds
            size = npcCollider.bounds.size;
            center = npcCollider.bounds.center - npc.npcTransform.position;
        }
        
        // Position the collider on top of the NPC's head
        float npcHeight = size.y;
        Vector3 topPosition = npc.npcTransform.position + new Vector3(0, npcHeight / 2 + center.y, 0);
        
        // Create a thin box collider just above the NPC's head
        boxCollider.size = new Vector3(size.x, npcExtraHeight, size.z);
        
        // Position it just above the NPC's head
        npc.tempCollider.transform.position = topPosition + new Vector3(0, npcExtraHeight / 2, 0);
        npc.tempCollider.transform.rotation = npc.npcTransform.rotation;
        
                    // Debug visualization
        if (showNPCDebugVisuals)
        {
            Debug.DrawLine(npc.npcTransform.position, topPosition, Color.yellow);
            // Wire cube visualization needs to be in OnDrawGizmos
            if (Application.isPlaying)
            {
                // Store debug info for drawing in OnDrawGizmos
                debugCubePositions.Add(npc.tempCollider.transform.position);
                debugCubeSizes.Add(boxCollider.size);
            }
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw detection radius in edit mode
        if (!Application.isPlaying)
        {
            if (preventNPCStepping)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, npcDetectionRadius);
            }
            
            if (preventCameraClipping && GetComponentInChildren<Camera>() != null)
            {
                Transform camTransform = GetComponentInChildren<Camera>().transform;
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawSphere(camTransform.position, clipDistance);
            }
            
            return;
        }
        
        // Draw NPC detection radius
        if (preventNPCStepping)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, npcDetectionRadius);
            
            // Draw debug cubes for NPC head blockers
            Gizmos.color = Color.red;
            for (int i = 0; i < debugCubePositions.Count; i++)
            {
                if (i < debugCubeSizes.Count)
                {
                    Gizmos.DrawWireCube(debugCubePositions[i], debugCubeSizes[i]);
                }
            }
        }
        
        // Draw camera collision check sphere
        if (preventCameraClipping && cameraTransform != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawSphere(cameraTransform.position, clipDistance);
        }
    }
    
    private void LateUpdate()
    {
        // Clear debug visualization data each frame
        debugCubePositions.Clear();
        debugCubeSizes.Clear();
        
        // Handle mouse look
        HandleMouseLook();
        
        // Handle movement
        HandleMovement();
        
        // Handle gravity and jumping
        HandleGravityAndJump();
        
        // Handle camera collision prevention
        if (preventCameraClipping)
        {
            HandleCameraCollision();
        }
        
        // Handle NPC collision prevention
        if (preventNPCStepping)
        {
            HandleNPCCollision();
        }
    }

    private void OnDestroy()
    {
        // Clean up all temporary colliders
        if (preventNPCStepping)
        {
            foreach (NPCCollider npc in npcColliders)
            {
                if (npc.tempCollider != null)
                {
                    Destroy(npc.tempCollider);
                }
            }
        }
    }
}