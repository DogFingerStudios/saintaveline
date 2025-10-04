using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatFloat : MonoBehaviour
{
    [SerializeField] private Transform[] _floatPoints;
    [SerializeField] private float _waterLevel = 0f;
    [SerializeField] private float _floatForce = 10f;
    [SerializeField] private float _damping = 0.1f;
    [SerializeField] private float _waveAmplitude = 0.5f;
    [SerializeField] private float _waveFrequency = 1f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (Transform point in _floatPoints)
        {
            Vector3 pos = point.position;
            float wave = Mathf.Sin(Time.time * _waveFrequency + pos.x + pos.z) * _waveAmplitude;
            float waterHeight = _waterLevel + wave;

            if (pos.y < waterHeight)
            {
                float displacement = waterHeight - pos.y;
                Vector3 upwardForce = Vector3.up * _floatForce * displacement;
                _rb.AddForceAtPosition(upwardForce - _rb.GetPointVelocity(pos) * _damping, pos, ForceMode.Acceleration);
            }
        }
    }
}
