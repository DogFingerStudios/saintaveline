// The boat code in general is a rough AI slop, but we need to move on.
// Ideally this script would be merged with BoatFloa (it could use the
// same sample points, but not necessarily). The water detection should 
// detect three states: (a) a point under water, (b) a point over land,
// (c) a point over land. There should be better beached mechanics, such
// that when the boat is beached it can still be driveable (but very 
// slowly). Also, it would be good to add other mechanics such as sinking,
// jumping ramps, etc. But for now this is good enough.

using UnityEngine;

public class BoatWaterDetector : MonoBehaviour
{
    [Header("Samples")]
    [SerializeField] private Transform[] _samplePoints = null;      // AI: reuse your boat float points or keel points
    public Transform[] SamplePoints { get { return _samplePoints; } }

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

   public int WaterHits;

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

        // The code inside this loop is mostly shit. It's some of the most shameful shit
        // I have ever written. And let it be known that this is by no means a reflection
        // of @[Not Sure] or @Matthew -- this is entirely on me. You would think that 
        // after the 40+ years I've been writing code that I wouldn't write shit like this,
        // but here were are. I only hope that @[Tomorrow Addy] can redeem himself and 
        // come back to this and write something better. We shall. Time will tell.
        //
        // Yours in Christ,
        // @[Today Addy]
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
                if (waterDepth < minGround) minGround = waterDepth;
            }

            // inWater = minGround > _waterTransform.position.y; 
            inWater = p.position.y < _waterTransform.position.y;

            if (inWater)
            {
                waterHits++;
                depthSum += waterDepth;
            }
        }

        WaterHits = waterHits;

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
