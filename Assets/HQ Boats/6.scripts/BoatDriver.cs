// AI: BoatDriver.cs - throttle, rudder, and camera handoff for piloting a floating boat
// AI: Unity 6000.0.43f1, ASCII only, braces on new lines, private fields use underscore.

using System.Threading;
using UnityEngine;

[DefaultExecutionOrder(0)]
[RequireComponent(typeof(Rigidbody))]
public class BoatDriver : MonoBehaviour
{
    [Header("Engine")]
    [SerializeField] private float _maxThrust = 15000f;            // AI: Newtons at full ahead
    [SerializeField] private float _reverseThrustFactor = 0.4f;    // AI: reverse is weaker
    [SerializeField] private float _throttleChangeRate = 1.5f;     // AI: units per second toward target
    [SerializeField] private Transform _propulsorPoint = null;     // AI: where thrust is applied (stern)

    [Header("Rudder / Turning")]
    [SerializeField] private float _rudderTorque = 6000f;          // AI: yaw torque scale
    [SerializeField] private float _rudderResponse = 3.0f;         // AI: steer smoothing rate
    [SerializeField] private float _maxRudderVisual = 25f;         // AI: deg, optional visual wheel/helm
    [SerializeField] private Transform _steeringWheelVisual = null;

    [Header("Hydrodynamic Drag")]
    [SerializeField] private float _lateralWaterResistance = 1200f;   // AI: fights side slip (quadratic-ish)
    [SerializeField] private float _longitudinalDrag = 180f;          // AI: limits top speed (quadratic-ish)
    [SerializeField] private float _brakeDragMultiplier = 3.0f;       // AI: extra drag when Space is held

    [Header("Pilot Handoff")]
    [SerializeField] private Transform _pilotCameraAnchor = null;   // AI: camera mount while piloting
    [SerializeField] private float _cameraLerp = 10f;               // AI: how fast camera snaps to anchor

    [Header("Input Map (WASD)")]
    [SerializeField] private KeyCode _enterExitKey = KeyCode.E;
    [SerializeField] private KeyCode _throttleForwardKey = KeyCode.W;
    [SerializeField] private KeyCode _throttleReverseKey = KeyCode.S;
    [SerializeField] private KeyCode _steerLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _steerRightKey = KeyCode.D;
    [SerializeField] private KeyCode _brakeKey = KeyCode.Space;

    // AI: runtime state
    private Rigidbody _rb;
    private bool _isPiloting = false;
    private float _throttleTarget = 0f; // AI: -1..+1
    private float _throttle = 0f;       // AI: -1..+1 smoothed
    private float _steerTarget = 0f;    // AI: -1..+1
    private float _steer = 0f;          // AI: -1..+1 smoothed
    private Transform _playerRoot = null;
    private MonoBehaviour _disabledMovementA = null; // AI: optional player movement component to disable
    private MonoBehaviour _disabledMovementB = null; // AI: optional secondary component
    private Camera _playerCamera = null;
    private Transform _originalCameraParent = null;
    private Vector3 _originalCamLocalPos;
    private Quaternion _originalCamLocalRot;

    private FPSMovement? _fpsMovement;

    private void Awake()
    {
        _fpsMovement = GameObject.FindGameObjectWithTag("Player")?.GetComponent<FPSMovement>();

        _rb = GetComponent<Rigidbody>();

        // AI: reasonable Rigidbody defaults for surface craft
        _rb.linearDamping = Mathf.Max(_rb.linearDamping, 0.5f);
        _rb.angularDamping = Mathf.Max(_rb.angularDamping, 4f);
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private float _mouseSensitivity = 2f;
    private float _maxLookAngle = 60f;
    private float _xRotation = 0f;
    private float _yRotation = 0f;

    private void Update()
    {
        if (_isPiloting)
        {
            // AI: read inputs
            int t = 0;
            if (Input.GetKey(_throttleForwardKey))
            {
                t += 1;
            }
            if (Input.GetKey(_throttleReverseKey))
            {
                t -= 1;
            }
            _throttleTarget = Mathf.Clamp(t, -1, 1);

            int s = 0;
            if (Input.GetKey(_steerLeftKey))
            {
                s -= 1;
            }
            if (Input.GetKey(_steerRightKey))
            {
                s += 1;
            }
            _steerTarget = Mathf.Clamp(s, -1, 1);

            // AI: smooth control
            _throttle = Mathf.MoveTowards(_throttle, _throttleTarget, _throttleChangeRate * Time.deltaTime);
            _steer = Mathf.MoveTowards(_steer, _steerTarget, _rudderResponse * Time.deltaTime);

            // AI: optional exit
            //if (Input.GetKeyDown(_enterExitKey))
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndPiloting();
            }
        }

        // AI: camera follow while piloting
        if (_isPiloting && _playerCamera != null && _pilotCameraAnchor != null)
        {
            _playerCamera.transform.position = Vector3.Lerp(
                _playerCamera.transform.position,
                _pilotCameraAnchor.position,
                1f - Mathf.Exp(-_cameraLerp * Time.deltaTime)
            );

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

            _yRotation -= mouseX;
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -_maxLookAngle, _maxLookAngle);
            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, -_yRotation, 0f);
        }

        // AI: visual wheel turn
        if (_steeringWheelVisual != null)
        {
            float angle = _steer * _maxRudderVisual;
            _steeringWheelVisual.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }

    private void FixedUpdate()
    {
        // AI: apply propulsion and hydrodynamics only when piloting
        if (!_isPiloting)
        {
            return;
        }

        // AI: compute thrust
        float thrust = _throttle >= 0f ? _throttle * _maxThrust : _throttle * _maxThrust * _reverseThrustFactor;

        Vector3 thrustDir = transform.forward;
        Vector3 thrustPos = _propulsorPoint != null ? _propulsorPoint.position : transform.position;

        _rb.AddForceAtPosition(thrustDir * thrust, thrustPos, ForceMode.Force);

        // AI: rudder torque scales with forward speed to avoid spinning in place
        Vector3 vLocal = transform.InverseTransformDirection(_rb.linearVelocity);
        float speedFactor = Mathf.Clamp01(Mathf.Abs(vLocal.z) / 5f); // AI: scale onset around 5 m/s
        float yawTorque = _steer * _rudderTorque * (0.3f + 0.7f * speedFactor);
        _rb.AddTorque(Vector3.up * yawTorque, ForceMode.Force);

        // AI: quadratic-like water resistance
        // AI: lateral (sideways) resistance
        Vector3 lateralLocal = new Vector3(vLocal.x, 0f, 0f);
        Vector3 lateralWorld = transform.TransformDirection(lateralLocal);
        Vector3 lateralDrag = -lateralWorld * _lateralWaterResistance * Mathf.Abs(vLocal.x);
        _rb.AddForce(lateralDrag, ForceMode.Force);

        // AI: longitudinal drag
        Vector3 longWorld = transform.forward * vLocal.z;
        Vector3 longDrag = -longWorld * _longitudinalDrag * Mathf.Abs(vLocal.z);
        _rb.AddForce(longDrag, ForceMode.Force);

        // AI: brake drag
        if (Input.GetKey(_brakeKey))
        {
            _rb.AddForce(-_rb.linearVelocity * _brakeDragMultiplier, ForceMode.Force);
        }
    }

    // AI: call this to begin piloting; provide player root and its camera
    public void BeginPiloting(Transform playerRoot, Camera playerCamera)
    {
        if (_isPiloting)
        {
            return;
        }

        _playerRoot = playerRoot;
        _playerCamera = playerCamera;

        // AI: disable common movement scripts if present; extend if you use different names
        //_disabledMovementA = TryDisable<MonoBehaviour>(_playerRoot, "FPSMovement");
        _disabledMovementB = TryDisable<MonoBehaviour>(_playerRoot, "FirstPersonController");

        // AI: parent camera to keep it local to the boat while blending
        if (_playerCamera != null)
        {
            _originalCameraParent = _playerCamera.transform.parent;
            _originalCamLocalPos = _playerCamera.transform.localPosition;
            _originalCamLocalRot = _playerCamera.transform.localRotation;
            _playerCamera.transform.SetParent(null, true);
        }

        _throttleTarget = 0f;
        _throttle = 0f;
        _steerTarget = 0f;
        _steer = 0f;

        _isPiloting = true;
        _fpsMovement.IsInDrivingMode = true;
    }

    // AI: call to exit piloting and restore player
    public void EndPiloting()
    {
        if (!_isPiloting)
        {
            return;
        }

        _isPiloting = false;

        // AI: restore camera
        if (_playerCamera != null && _originalCameraParent != null)
        {
            _playerCamera.transform.SetParent(_originalCameraParent, true);
            _playerCamera.transform.localPosition = _originalCamLocalPos;
            _playerCamera.transform.localRotation = _originalCamLocalRot;
        }

        // AI: re-enable movement scripts
        if (_disabledMovementA != null)
        {
            _disabledMovementA.enabled = true;
        }
        if (_disabledMovementB != null)
        {
            _disabledMovementB.enabled = true;
        }

        _playerRoot = null;
        _playerCamera = null;
        _disabledMovementA = null;
        _disabledMovementB = null;
        _originalCameraParent = null;

        _fpsMovement.IsInDrivingMode = false;
    }

    // AI: utility to find and disable a component by type name
    private T TryDisable<T>(Transform root, string typeName) where T : MonoBehaviour
    {
        if (root == null)
        {
            return null;
        }
        T[] list = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < list.Length; i++)
        {
            T c = list[i];
            if (c != null && c.GetType().Name == typeName)
            {
                c.enabled = false;
                return c;
            }
        }
        return null;
    }

    // AI: convenience for external systems
    public bool IsPiloting()
    {
        return _isPiloting;
    }

    public float CurrentThrottle01()
    {
        return (_throttle + 1f) * 0.5f;
    }

    public float CurrentSteer01()
    {
        return (_steer + 1f) * 0.5f;
    }
}
