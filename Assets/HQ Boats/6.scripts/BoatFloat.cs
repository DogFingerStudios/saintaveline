// AI: BoatFloat.cs - stabilized buoyancy, vertical springs only, smoothed righting (Unity 6000.0.43f1)
// AI: ASCII only. No Unicode.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    // AI: Four or six float points, placed symmetrically near the waterline
    [Header("Float Points")]
    [SerializeField] private Transform[] _floatPoints = null;

    [Header("Water")]
    [SerializeField] private float _waterLevel = 0f;
    //[SerializeField] private float _waveAmplitude = 0.3f;
    [SerializeField] private float _waveAmplitude = 0.0f;
    [SerializeField] private float _waveFrequency = 1.1f;

    [Header("Buoyancy Shaping")]
    [SerializeField] private float _targetBounceFrequency = 0.9f;   // AI: Hz; lower = softer springs = boat sits deeper
    [SerializeField] private float _dampingRatio = 0.95f;           // AI: near-critical
    [SerializeField] private float _maxSubmergeDepth = 0.9f;        // AI: per-point clamp

    [Header("Force Caps")]
    [SerializeField] private float _perPointForceCap = 0f;          // AI: 0 = auto ~ 2.5x weight-per-point
    [SerializeField] private float _maxUpwardVelocity = 5.0f;       // AI: anti-rocket clamp

    [Header("Drag (Separated)")]
    [SerializeField] private float _verticalDrag = 6.0f;            // AI: vertical damping
    [SerializeField] private float _lateralDrag = 0.5f;             // AI: horizontal damping

    [Header("Stability (Righting)")]
    [SerializeField] private float _rightingStrength = 240f;        // AI: roll/pitch torque magnitude
    [SerializeField] private float _rightingDamping = 100f;         // AI: angular damping
    [SerializeField] private float _normalSmooth = 8f;              // AI: smoothing rate for water normal

    [Header("Startup")]
    [SerializeField] private float _warmupTime = 0.75f;             // AI: ramp buoyancy from 0 to 1 over this time

    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = false;

    // AI: Internals
    private Rigidbody _rb;
    private float _k;                                               // AI: spring strength per point
    private float _c;                                               // AI: damping per point
    private float _t0;
    private Vector3 _smoothedWaterNormal = Vector3.up;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // AI: Unity 6000 physics fields
        _rb.linearDamping = 0.5f;
        _rb.angularDamping = 4f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // AI: Lower center of mass for stability
        _rb.centerOfMass = new Vector3(0f, -0.6f, 0f);

#if UNITY_6000_0_OR_NEWER
        // AI: Harden solver a bit
        if (_rb.solverIterations < 12)
        {
            _rb.solverIterations = 12;
        }
        if (_rb.solverVelocityIterations < 6)
        {
            _rb.solverVelocityIterations = 6;
        }
#endif

        ComputeSpring();
        AutoCapIfNeeded();

        _t0 = Time.time;
        _smoothedWaterNormal = Vector3.up;
    }

    private void OnValidate()
    {
        // AI: Clamp inspector values
        if (_targetBounceFrequency < 0.1f)
        {
            _targetBounceFrequency = 0.1f;
        }
        if (_dampingRatio < 0.1f)
        {
            _dampingRatio = 0.1f;
        }
        if (_maxSubmergeDepth < 0.05f)
        {
            _maxSubmergeDepth = 0.05f;
        }
        if (_normalSmooth < 0.1f)
        {
            _normalSmooth = 0.1f;
        }
    }

    private void ComputeSpring()
    {
        int n = Mathf.Max(1, _floatPoints != null ? _floatPoints.Length : 1);
        float mEff = Mathf.Max(1e-3f, _rb != null ? _rb.mass / n : 1f); // AI: effective mass per spring
        float w = Mathf.PI * 2f * _targetBounceFrequency;               // AI: rad/s
        _k = mEff * w * w;                                              // AI: k = m * w^2
        _c = 2f * _dampingRatio * Mathf.Sqrt(_k * mEff);                // AI: c = 2 * zeta * sqrt(k * m)
    }

    private void AutoCapIfNeeded()
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
        // AI: Smoothstep 0..1
        return t * t * (3f - 2f * t);
    }

    private float WaterHeightAt(in Vector3 wp)
    {
        // AI: Simple placeholder wave. Replace with your water API if available.
        return _waterLevel + Mathf.Sin(Time.time * _waveFrequency + wp.x * 0.15f + wp.z * 0.18f) * _waveAmplitude;
    }

    private Vector3 SmoothedWaterNormal(in Vector3 sampleCenter)
    {
        // AI: Sample a larger patch to reduce noise, then exponential smooth
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
        if (_floatPoints == null || _floatPoints.Length == 0)
        {
            return;
        }

        float ramp = WarmupScale();

        // AI: Use smoothed water normal for righting torque only
        Vector3 center = transform.position;
        Vector3 waterN = SmoothedWaterNormal(center);

        int submerged = 0;

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

            submerged++;

            // AI: Vertical-only spring
            float clampedDepth = Mathf.Min(depth, _maxSubmergeDepth);
            float spring = _k * clampedDepth;

            Vector3 vPoint = _rb.GetPointVelocity(wp);

            // AI: Vertical damping only
            float vY = Vector3.Dot(vPoint, Vector3.up);
            float damp = _c * vY;

            float forceY = Mathf.Clamp((spring - damp) * ramp, 0f, _perPointForceCap);
            Vector3 buoyant = Vector3.up * forceY;

            // AI: Separated drag
            Vector3 vLateral = vPoint - Vector3.Project(vPoint, Vector3.up);
            Vector3 drag = (-Vector3.up * vY * _verticalDrag) + (-vLateral * _lateralDrag);

            _rb.AddForceAtPosition(buoyant + drag, wp, ForceMode.Force);
        }

        // AI: Vertical speed clamp
        Vector3 lv = _rb.linearVelocity;
        if (lv.y > _maxUpwardVelocity)
        {
            lv.y = _maxUpwardVelocity;
            _rb.linearVelocity = lv;
        }

        // AI: Smoothed righting torque - correct roll and pitch only
        if (submerged > 0)
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
