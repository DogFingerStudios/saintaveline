using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private GameObject _objectToMove = null!;
    [SerializeField] private Vector3 _moveDirection = Vector3.forward;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _horizonStartDistance = 0f;
    [SerializeField] private float _horizonDropDistance = 10f;
    [SerializeField] private float _horizonDropAmount = 1f;

    private Vector3 _initialPosition;
    private float _distanceTravelled;

    private void Start()
    {
        if (_objectToMove == null)
        {
            _objectToMove = gameObject;
        }

        _initialPosition = _objectToMove.transform.position;
    }

    private void Update()
    {
        Vector3 frameMovement = _moveDirection.normalized * (_moveSpeed * Time.deltaTime);
        _objectToMove.transform.position += frameMovement;

        _distanceTravelled += frameMovement.magnitude;

        float normalizedDistance = Mathf.Clamp01((_distanceTravelled - _horizonStartDistance) / Mathf.Max(_horizonDropDistance, 0.0001f));
        float yOffset = -_horizonDropAmount * normalizedDistance;

        Vector3 updatedPosition = _objectToMove.transform.position;
        updatedPosition.y = _initialPosition.y + yOffset;
        _objectToMove.transform.position = updatedPosition;
    }
}
