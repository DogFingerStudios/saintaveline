using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    private Transform _transform;
    private Camera _camera;

    private void Awake()
    {
        _transform = transform;
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_transform != null && _camera != null)
        {
            _transform.rotation = _camera.transform.rotation;
        }
    }

}