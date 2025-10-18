using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _camera;

    private float _minSize = 10f;
    private float _maxSize = 500f;
    private float _zoomSpeed = 10f;

    void Update()
    {
        Vector3 newPosition = _target.transform.position;
        newPosition.y = 100f;
        this.transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Minus) && _camera.orthographicSize < _maxSize)
        {
            _camera.orthographicSize += _zoomSpeed;
        }
        else if (Input.GetKeyDown(KeyCode.Equals) && _camera.orthographicSize > _minSize)
        {
            _camera.orthographicSize -= _zoomSpeed;
        }

        //Vector3 newRotation = this.transform.eulerAngles;
        //newRotation.y = _target.transform.eulerAngles.y;
        //this.transform.eulerAngles = newRotation;
    }
}
