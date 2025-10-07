// AI: BoatPilotStation.cs - simple "press E while looking at wheel" interactor for piloting toggle
// AI: Attach to the steering wheel object. Assign _boat reference in Inspector.

using UnityEngine;

[DefaultExecutionOrder(10)]
public class BoatPilotStation : MonoBehaviour
{
    [SerializeField] private BoatDriver _boat = null;
    [SerializeField] private float _useDistance = 3.0f;          // AI: how far you can start piloting
    [SerializeField] private LayerMask _rayMask = ~0;            // AI: optional filter; default hits everything
    [SerializeField] private KeyCode _useKey = KeyCode.E;

    private Camera _cam;
    private Transform _player;

    private void Start()
    {
        _cam = Camera.main;
        if (_cam != null)
        {
            _player = _cam.transform; // AI: fallback for distance gating
        }
    }

    private void Update()
    {
        if (_boat == null || _cam == null)
        {
            return;
        }

        if (Input.GetKeyDown(_useKey))
        {
            if (_boat.IsCoolingDown())
            {
                return;
            }
            
            if (_boat.IsPiloting())
            {
                _boat.EndPiloting();
                return;
            }

            // AI: ray from center of screen to detect steering wheel
            Ray r = new Ray(_cam.transform.position, _cam.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, _useDistance, _rayMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    // AI: distance gate to reduce accidental use
                    if (_player != null)
                    {
                        float d = Vector3.Distance(_player.position, transform.position);
                        if (d > _useDistance + 0.5f)
                        {
                            return;
                        }
                    }

                    _boat.BeginPiloting(GetPlayerRoot(), _cam);
                }
            }
        }
    }

    // AI: attempts to find the logical player root from the main camera
    private Transform GetPlayerRoot()
    {
        Transform t = _cam.transform;
        // AI: climb a few parents to escape the camera rig
        for (int i = 0; i < 4 && t.parent != null; i++)
        {
            t = t.parent;
        }
        return t;
    }
}
