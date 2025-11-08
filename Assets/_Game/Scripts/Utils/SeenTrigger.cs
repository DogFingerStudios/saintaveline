using UnityEngine;
using UnityEngine.Events;
using System;
using Unity.Mathematics;

/// <summary>
/// Attach to any GameObject that should react when the player
/// looks at it and then looks away (while being close enough).
/// </summary>
[RequireComponent(typeof(Collider))] // for distance check (can be trigger or non-trigger)
public class SeenTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Maximum distance from player to be considered 'close'")]
    public float maxDistance = 8f;

    [Tooltip("How long (seconds) the object must be visible before we arm the 'looked away' event")]
    public float minVisibleTime = 0.5f;

    [Tooltip("Optional: only count if the dot product (forward direction) is above this")]
    [Range(0f, 1f)] public float minDot = 0.5f; // 0 = any angle, 1 = straight ahead

    [Header("What to do when the player looks away")]
    public UnityEvent onLookedAway = new UnityEvent();

    // ---- private state -------------------------------------------------
    private Transform playerCam;
    private float visibleTimer = 0f;
    private bool wasVisibleLastFrame = false;
    private bool armed = false; // true after minVisibleTime

    private void Awake()
    {
        // Find the main camera (or a custom FPS controller camera)
        playerCam = Camera.main?.transform;
        if (playerCam == null)
        {
            throw new Exception("There is no Camera");
        }
    }

    private void Update()
    {
        if (playerCam == null) return;

        bool inRange = Vector3.Distance(playerCam.position, transform.position) <= maxDistance;
        if (!inRange)
        {
            ResetState();
            return;
        }

        bool currentlyVisible = IsInView();

        if (currentlyVisible)
        {
            visibleTimer += Time.deltaTime;
            if (visibleTimer >= minVisibleTime)
                armed = true;
        }
        else
        {
            visibleTimer = 0f; // reset timer while out of view
        }

        // ---- Detect the *transition* out of view ----
        if (wasVisibleLastFrame && !currentlyVisible && armed)
        {
            onLookedAway.Invoke(); // <-- your lambda / action
            armed = false; // fire only once per look-away
        }

        wasVisibleLastFrame = currentlyVisible;
    }

    private bool IsInView()
    {
        // 1. Frustum test (GeometryUtility)
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        if (!GeometryUtility.TestPlanesAABB(planes, GetComponent<Collider>().bounds))
        {
            return false;
        }

        // 2. Optional forward-dot test
        Vector3 toObj = transform.position - playerCam.position;
        float dot = Vector3.Dot(playerCam.forward, toObj.normalized);
        if (dot < minDot) return false;

        // 3. (Optional) Occlusion test with a tiny Physics.SphereCast
        // Uncomment if you need to ignore objects that are behind walls.
        /*
        if (Physics.SphereCast(playerCam.position, 0.1f, toObj, out RaycastHit hit, toObj.magnitude))
        {
            if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                return false;
        }
        */

        return true;
    }

    private void ResetState()
    {
        visibleTimer = 0f;
        armed = false;
        wasVisibleLastFrame = false;
    }

    // ---------------------------------------------------------------
    // Inspector convenience: assign a lambda directly from another script
    [ContextMenu("Clear Event")]
    private void ClearEvent() => onLookedAway.RemoveAllListeners();

    public void AddListener(UnityAction action) => onLookedAway.AddListener(action);
    public void RemoveListener(UnityAction action) => onLookedAway.RemoveListener(action);
}