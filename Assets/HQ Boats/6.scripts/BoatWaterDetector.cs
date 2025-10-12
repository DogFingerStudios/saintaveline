// AI: BoatWaterDetector.cs - robust water vs land detection only (no disabling). Unity 6000.0.43f1
// AI: ASCII only. Braces on new lines. Private fields use underscore.
// AI: Two detection paths:
// AI:   1) Collider mode: SphereCast down to find Water and Ground colliders (recommended if your water has a collider/trigger).
// AI:   2) Height API mode: Sample a water height provider to compute submergence without colliders.

using UnityEngine;

public class BoatWaterDetector : MonoBehaviour
{
    [Header("Samples")]
    [SerializeField] private Transform[] _samplePoints = null;      // AI: reuse your boat float points or keel points

    [Header("Collider Mode")]
    [SerializeField] private bool _useColliderMode = true;          // AI: true = cast vs colliders; false = use height provider
    [SerializeField] private LayerMask _waterMask = 1 << 4;         // AI: assign your Water layer
    [SerializeField] private LayerMask _groundMask = ~0;            // AI: assign Ground/Terrain layers
    [SerializeField] private float _probeRadius = 0.25f;            // AI: sphere radius
    [SerializeField] private float _probeDepth = 3.0f;              // AI: cast depth below each sample point

    [Header("Decision Thresholds")]
    [SerializeField] private float _waterLevel = 0f;
    [SerializeField] private float _minWaterDepth = 0.20f;          // AI: meters of water needed under sample to count as water
    [SerializeField] private float _requiredCoverage = 0.65f;        // AI: fraction of samples that must be in water
    [SerializeField] private float _beachClearance = 0.15f;         // AI: ground within this distance means beached

    [Header("Water Transform")]
    [SerializeField] private Transform _waterTransform;

    // AI: Public readouts
    public bool IsOnWater { get { return _isOnWater; } }
    public bool IsOverland { get { return _isOverland; } }
    public bool IsBeached { get { return _isBeached; } }
    public float WaterCoverage01 { get { return _coverage01; } }    // AI: 0..1 fraction of samples in water
    public float AvgWaterDepth { get { return _avgWaterDepth; } }   // AI: average depth under samples (m)
    public float MinGroundClearance { get { return _minGroundClear; } } // AI: smallest ground distance (m)

    private bool _isOnWater;
    private bool _isOverland;
    private bool _isBeached;
    private float _coverage01;
    private float _avgWaterDepth;
    private float _minGroundClear;


    private void Update()
    {
        if (_samplePoints == null || _samplePoints.Length == 0)
        {
            _isOnWater = false;
            _isOverland = true;
            _isBeached = false;
            _coverage01 = 0f;
            _avgWaterDepth = 0f;
            _minGroundClear = float.PositiveInfinity;
            return;
        }

        int waterHits = 0;
        float depthSum = 0f;
        float minGround = float.PositiveInfinity;

        for (int i = 0; i < _samplePoints.Length; i++)
        {
            Transform p = _samplePoints[i];
            if (p == null)
            {
                continue;
            }

            Vector3 origin = p.position + Vector3.up * 0.05f;
            float waterDepth = 0f;
            bool inWater;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 500, _groundMask))
            {
                waterDepth = hit.distance;
                if (waterDepth < minGround)
                    minGround = waterDepth;
            }

            // MATT: Water level should be the water transform Y
            inWater = minGround > _waterTransform.position.y; // TODO: do not HARD CODE this

            if (inWater)
            {
                waterHits++;
                depthSum += waterDepth;
            }
        }

        // the percentage of sample points that are in water
        _coverage01 = Mathf.Clamp01((float)waterHits / Mathf.Max(1, _samplePoints.Length));

        _avgWaterDepth = waterHits > 0 ? depthSum / waterHits : 0f;
        // _minGroundClear = float.IsPositiveInfinity(minGround) ? _probeDepth : minGround;

        // to be considered "on water", the boat must have at least `_requiredCoverage` of its samples in water
        // (where `_requiredCoverage` is a fraction of points) and the average water depth `_minWaterDepth`.
        _isOnWater = (_coverage01 >= _requiredCoverage) && (_avgWaterDepth >= _minWaterDepth);

        _isBeached = (_coverage01 > 0f) && (_coverage01 < 1f); // && (_minGroundClear <= _beachClearance);

        // _isOverland = (_coverage01 < 0.01f) && (_minGroundClear <= _probeDepth * 0.9f);
        _isOverland = (_coverage01 < 0.01f);
    }
}
