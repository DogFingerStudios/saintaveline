using UnityEngine;

/// <summary>
/// Comprehensive camera clip prevention for first-person cameras.
/// Uses a spherical raycast approach to prevent clipping through any surface.
/// Attach this script directly to your first-person camera.
/// </summary>
public class FirstPersonCameraClipPrevention : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Layers that the camera will check for collision")]
    [SerializeField] private LayerMask collisionLayers = ~0; // Default: collide with everything
    
    [Tooltip("Distance from camera center to check for collisions")]
    [SerializeField] private float clipDistance = 0.2f;
    
    [Tooltip("How far to push the near clip plane when colliding")]
    [SerializeField] private float minNearClipPlane = 0.005f;
    
    [Tooltip("Default near clip plane distance when not colliding")]
    [SerializeField] private float defaultNearClipPlane = 0.03f;
    
    [Tooltip("Number of sample points on the sphere (higher = more accurate but more expensive)")]
    [SerializeField] private int sampleCount = 30;

    // Camera component reference
    private Camera cam;
    
    // Store original values
    private float originalNearClipPlane;
    
    // Directions to check for collisions
    private Vector3[] checkDirections;
    
    // Debug visualization
    private bool isColliding = false;
    private float currentDistance = 0f;

    private void Awake()
    {
        // Get camera component
        cam = GetComponent<Camera>();
        
        if (cam == null)
        {
            Debug.LogError("FirstPersonCameraClipPrevention script must be attached to a Camera object!");
            enabled = false;
            return;
        }
        
        // Store original near clip plane value
        originalNearClipPlane = cam.nearClipPlane;
        
        // Generate directions for spherical checks
        GenerateCheckDirections();
    }

    // Generate evenly distributed points on a sphere using the Fibonacci sphere algorithm
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

    private void LateUpdate()
    {
        // Reset collision state
        isColliding = false;
        currentDistance = float.MaxValue;
        
        // First do a sphere cast to catch any close collisions
        if (Physics.CheckSphere(transform.position, clipDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // If we're inside a collider, use a very small near clip plane
            cam.nearClipPlane = minNearClipPlane;
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
            cam.nearClipPlane = adjustedClipPlane;
        }
        else
        {
            // Restore default near clip plane when not colliding
            cam.nearClipPlane = defaultNearClipPlane;
        }
    }

    private void CheckCameraCollisions()
    {
        // Cast rays in all pre-computed directions
        foreach (Vector3 direction in checkDirections)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out RaycastHit hit, clipDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                // We hit something - record collision
                isColliding = true;
                
                // Keep track of the closest collision
                if (hit.distance < currentDistance)
                {
                    currentDistance = hit.distance;
                }
                
                // Draw debug ray for visualization
                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                // Draw debug ray for rays that don't hit anything
                Debug.DrawRay(transform.position, transform.TransformDirection(direction) * clipDistance, Color.green);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && enabled)
        {
            // Visualize the raycast check volume
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawSphere(transform.position, clipDistance);
            
            // If we have directions already calculated in edit mode, show them
            if (checkDirections != null && checkDirections.Length > 0)
            {
                foreach (Vector3 direction in checkDirections)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(transform.position, transform.TransformDirection(direction) * clipDistance);
                }
            }
        }
    }
}