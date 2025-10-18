// AI: Disable global fog only while this camera renders (Built-in RP)
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class MinimapFogOverride : MonoBehaviour
{
    [SerializeField] private Light _minimapSun;
    [SerializeField] private Transform _followTarget;

    private bool _sunWasEnabled;
    private bool _fogWasEnabled;
    private float _initialCameraY;

    public void Awake()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        _initialCameraY = transform.position.y;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Equals) && _initialCameraY < 8000)
        {
            _initialCameraY += 100;
        }
        else if (Input.GetKey(KeyCode.Minus) && _initialCameraY > 50)
        {
            _initialCameraY -= 100;
        }
    }

    private void LateUpdate()
    {
        if (_followTarget != null)
        {
            Vector3 newPosition = _followTarget.position;
            //newPosition.y = transform.position.y;
            newPosition.y = _initialCameraY;
            transform.position = newPosition;
        }
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        _fogWasEnabled = RenderSettings.fog;
        RenderSettings.fog = false;

        if (_minimapSun != null)
        {
            _sunWasEnabled = _minimapSun.enabled;
            _minimapSun.enabled = true;
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        RenderSettings.fog = _fogWasEnabled;

        if (_minimapSun != null)
        {
            _minimapSun.enabled = _sunWasEnabled;
        }
    }
}
