using UnityEngine;
using UnityEngine.Rendering;

public class MinimapLighting : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Light _minimapLight;
    [SerializeField] private Transform _target; // player transform

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

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == _camera)
        {
            _minimapLight.enabled = true;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == _camera)
        {
            _minimapLight.enabled = false;
        }
    }
}
