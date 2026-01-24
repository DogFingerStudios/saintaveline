using UnityEngine;
using UnityEngine.Rendering;

public class MinimapLighting : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private Camera _camera;
    [SerializeField] private Light _minimapLight;
    [SerializeField] private Transform _target; // player transform
    [SerializeField] private float _heightOffset = 50f;

    private Vector3 _cachedRot;
    private Vector3 _cachedPos;

    [SerializeField] private float _minSize = 10f;
    [SerializeField] private float _maxSize = 500f;
    [SerializeField] private float _zoomSpeed = 10f;

    public bool RotateWithPlayerDirection = false;


    private void Awake()
    {
        _transform = transform;
    }

    void Update()
    {
        _cachedPos = _target.transform.position;
        _cachedPos.y = _heightOffset;
        _transform.position = _cachedPos;

        if (Input.GetKeyDown(KeyCode.Minus) && _camera.orthographicSize < _maxSize)
        {
            _camera.orthographicSize += _zoomSpeed;
        }
        else if (Input.GetKeyDown(KeyCode.Equals) && _camera.orthographicSize > _minSize)
        {
            _camera.orthographicSize -= _zoomSpeed;
        }

        if (RotateWithPlayerDirection)
        {
            _cachedRot = _transform.eulerAngles;
            _cachedRot.y = _target.transform.eulerAngles.y;
            _transform.eulerAngles = _cachedRot;
        }
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
