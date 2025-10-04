// AI: BoatFloat.cs - stabilized buoyancy with vertical springs, smoothed righting, Unity 6000.0.43f1
// AI: ASCII only. Every block uses braces. Private fields prefixed with underscore.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    // AI: Place 4 or 6 points symmetrically near the hull waterline
    [Header("Float Points")]
    [SerializeField] private Transform[] _floatPoints = null;

    [Header("Water")]
    [SerializeField] private float _waterLevel = 0f;
    [SerializeField] private float _waveAmplitude = 0.0f;   // AI: set 0 for calibration, then 0.1..0.3
    [SerializeField] private float _waveFrequency = 1.0f;   // AI: Hz-ish feel

    [Header("Buoyancy Shaping")]
    [SerializeField] private float _targetBounceFrequency = 0.70f; // AI: Hz. Lower = softer spring = deeper ride
    [SerializeField] private float _dampingRatio = 1.05f;          // AI: >1 for overdamped startup calm
    [SerializeField] private float _maxSubmergeDepth = 0.60f;      // AI: clamp per point to avoid pops

    [Header("Force Caps")]
    [SerializeField] private float _perPointForceCap = 5500f;      // AI: ~2.5x weight/point for 900kg, 4 points
    [SerializeField] private float _maxUpwardVelocity = 5.0f;      // AI: anti-rocket clamp

    [Header("Drag (Separated)")]
    [SerializeField] private float _verticalDrag = 12.0f;          // AI: strong vertical damping to kill buzz
    [SerializeField] private float _lateralDrag = 0.20f;           // AI: mild horizontal damping

    [Header("Stability (Righting)")]
    [SerializeField] private float _rightingStrength = 160f;       // AI: torque magnitude toward smoothed water up
    [SerializeField] private float _rightingDamping = 160f;        // AI: angular velocity damping
    [SerializeField] private float _normalSmooth = 20f;            // AI: smoothing rate for water normal

    [Header("Startup")]
    [SerializeField] private float _warmupTime = 1.25f;            // AI: ramp buoyancy 0->1 to avoid impulse at start

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = false;

    // AI: internals
    private Rigidbody _rb;
    private float _k;                       // AI: spring strength per point
    private float _c;                       // AI: damping per point
    private float _t0;
    private Vector3 _smoothedWaterNormal = Vector3.up;
    private float _simTime = 0f;           // AI: physics-step time for wave evaluation

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // AI: Unity 6000 physics fields
        _rb.linearDamping = 0.7f;
        _rb.angularDamping = 6f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // AI: lower CoM for stability
        _rb.centerOfMass = new Vector3(0f, -0.50f, 0f);

#if UNITY_6000_0_OR_NEWER
        if (_rb.solverIterations < 20)
        {
            _rb.solverIterations = 20;
        }
        if (_rb.solverVelocityIterations < 10)
        {
            _rb.solverVelocityIterations = 10;
        }
#endif

        ComputeSpring();
        AutoForceCapIfNeeded();

        _t0 = Time.time;
        _smoothedWaterNormal = Vector3.up;
        _simTime = 0f;
    }

    private void OnValidate()
    {
        if (_targetBounceFrequency < 0.10f)
        {
            _targetBounceFrequency = 0.10f;
        }
        if (_dampingRatio < 0.10f)
        {
            _dampingRatio = 0.10f;
        }
        if (_maxSubmergeDepth < 0.05f)
        {
            _maxSubmergeDepth = 0.05f;
        }
        if (_normalSmooth < 0.10f)
        {
            _normalSmooth = 0.10f;
        }
    }

    private void ComputeSpring()
    {
        int n = Mathf.Max(1, _floatPoints != null ? _floatPoints.Length : 1);
        float mEff = Mathf.Max(1e-3f, _rb != null ? _rb.mass / n : 1f);
        float w = Mathf.PI * 2f * _targetBounceFrequency;
        _k = mEff * w * w;
        _c = 2f * _dampingRatio * Mathf.Sqrt(_k * mEff);
    }

    private void AutoForceCapIfNeeded()
    {
        if (_perPointForceCap <= 0f)
        {
            int n = Mathf.Max(1, _floatPoints != null ? _floatPoints.Length : 1);
            float weightPerPoint = (_rb.mass * Physics.gravity.magnitude) / n;
            _perPointForceCap = weightPerPoint * 2.5f;
        }
    }

    private float WarmupScale()
    {
        float t = Mathf.Clamp01((Time.time - _t0) / Mathf.Max(0.01f, _warmupTime));
        // AI: smoothstep 0..1
        return t * t * (3f - 2f * t);
    }

    private float WaterHeightAt(in Vector3 wp)
    {
        // AI: use physics-step time to avoid jitter from Time.time in FixedUpdate
        float phase = _simTime * _waveFrequency + wp.x * 0.15f + wp.z * 0.18f;
        return _waterLevel + Mathf.Sin(phase) * _waveAmplitude;
    }

    private Vector3 SmoothedWaterNormal(in Vector3 sampleCenter)
    {
        // AI: sample a larger patch and exponentially smooth
        float eps = 0.75f;

        float hC = WaterHeightAt(sampleCenter);
        float hX = WaterHeightAt(sampleCenter + new Vector3(eps, 0f, 0f));
        float hZ = WaterHeightAt(sampleCenter + new Vector3(0f, 0f, eps));

        Vector3 a = new Vector3(eps, hX - hC, 0f);
        Vector3 b = new Vector3(0f, hZ - hC, eps);

        Vector3 n = Vector3.Cross(b, a).normalized;

        float lerpRate = 1f - Mathf.Exp(-_normalSmooth * Time.fixedDeltaTime);
        _smoothedWaterNormal = Vector3.Slerp(_smoothedWaterNormal, n, lerpRate);
        return _smoothedWaterNormal;
    }

    private void FixedUpdate()
    {
        _simTime += Time.fixedDeltaTime;

        if (_floatPoints == null || _floatPoints.Length == 0)
        {
            return;
        }

        float ramp = WarmupScale();

        // AI: smoothed water normal only for righting torque
        Vector3 waterN = SmoothedWaterNormal(transform.position);

        int submergedCount = 0;

        for (int i = 0; i < _floatPoints.Length; i++)
        {
            Transform p = _floatPoints[i];
            if (p == null)
            {
                continue;
            }

            Vector3 wp = p.position;
            float waterH = WaterHeightAt(wp);
            float depth = waterH - wp.y;

            if (depth <= 0f)
            {
                continue;
            }

            submergedCount++;

            // AI: vertical-only spring and damping
            float clampedDepth = Mathf.Min(depth, _maxSubmergeDepth);
            float spring = _k * clampedDepth;

            Vector3 vPoint = _rb.GetPointVelocity(wp);

            float vY = Vector3.Dot(vPoint, Vector3.up);
            float damp = _c * vY;

            float forceY = Mathf.Clamp((spring - damp) * ramp, 0f, _perPointForceCap);
            Vector3 buoyant = Vector3.up * forceY;

            // AI: separated drag
            Vector3 vLateral = vPoint - Vector3.Project(vPoint, Vector3.up);
            Vector3 drag = (-Vector3.up * vY * _verticalDrag) + (-vLateral * _lateralDrag);

            _rb.AddForceAtPosition(buoyant + drag, wp, ForceMode.Force);
        }

        // AI: vertical speed clamp
        Vector3 lv = _rb.linearVelocity;
        if (lv.y > _maxUpwardVelocity)
        {
            lv.y = _maxUpwardVelocity;
            _rb.linearVelocity = lv;
        }

        // AI: smoothed righting torque, correct roll/pitch
        if (submergedCount > 0)
        {
            Vector3 up = transform.up;
            Vector3 targetUp = waterN;

            Vector3 axis = Vector3.Cross(up, targetUp);
            float mag = axis.magnitude;

            if (mag > 1e-4f)
            {
                Vector3 torque = axis.normalized * (_rightingStrength * mag) - (_rb.angularVelocity * _rightingDamping);
                _rb.AddTorque(torque, ForceMode.Force);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos || _floatPoints == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _floatPoints.Length; i++)
        {
            Transform p = _floatPoints[i];
            if (p == null)
            {
                continue;
            }
            Vector3 wp = p.position;
            Gizmos.DrawSphere(wp, 0.06f);
            Gizmos.DrawLine(wp, new Vector3(wp.x, _waterLevel, wp.z));
        }

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.2f);
        Gizmos.DrawCube(new Vector3(transform.position.x, _waterLevel, transform.position.z), new Vector3(8f, 0.02f, 8f));
    }
#endif
}
