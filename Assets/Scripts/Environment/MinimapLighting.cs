using UnityEngine;
using UnityEngine.Rendering;

public class MinimapLighting : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Light _globalLight;

    // Update is called once per frame
    void Update()
    {
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
            _globalLight.enabled = true;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == _camera)
        {
            _globalLight.enabled = false;
        }
    }
}
