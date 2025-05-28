// AI: Unity 6000.0.43f1
using UnityEngine;

public class MapLabeler : MonoBehaviour
{
    // AI: Prefab of ground marker, assigned in Inspector
    [SerializeField]
    private GameObject _circlePrefab;

    // AI: Layer mask for ground detection
    [SerializeField]
    private LayerMask _groundLayerMask;

    private GameObject _circleInstance;
    private Camera _mainCamera;
    private bool _labelModeActive;
    private Vector3 _lastHitPoint;
    private Vector3 _savedPosition;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            _labelModeActive = !_labelModeActive;

            if (_labelModeActive)
            {
                _circleInstance = Instantiate(_circlePrefab);
            }
            else if (_circleInstance != null)
            {
                Destroy(_circleInstance);
                _circleInstance = null;
            }
        }

        if (!_labelModeActive || _circleInstance == null)
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _groundLayerMask))
        {
            if (Vector3.Distance(hitInfo.point, _lastHitPoint) > 0.02f)
            {
                _circleInstance.transform.localScale = new Vector3(2f, 1.5f, 2f);
                _circleInstance.transform.position = hitInfo.point;
                _circleInstance.transform.position += Vector3.up * 0.01f;
                _lastHitPoint = hitInfo.point;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _savedPosition = hitInfo.point;
                Debug.Log("Saved position = " + _savedPosition);
                Destroy(_circleInstance);
                _circleInstance = null;
                _labelModeActive = false;
            }
        }
    }
}
