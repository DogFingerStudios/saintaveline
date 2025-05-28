using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapLabeler : MonoBehaviour
{
    [SerializeField] private GameObject _circlePrefab;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private GameObject _labelDialogPrefab;
    [SerializeField] private Canvas _uiCanvas;

    private GameObject _circleInstance;
    private GameObject _dialogInstance;

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

        // if (Input.GetKeyDown(KeyCode.Period))
        // {
        //     ShowLabelNameDialog();
        //     return;
        // }

        if (!_labelModeActive || _circleInstance == null)
            {
                return;
            }

        if (_labelModeActive
            && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            _labelModeActive = false;
            if (_circleInstance != null)
            {
                Destroy(_circleInstance);
                _circleInstance = null;
            }

            if (_dialogInstance != null)
            {
                Destroy(_dialogInstance);
                _dialogInstance = null;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

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
                ShowLabelNameDialog();
                // Debug.Log("Saved position = " + _savedPosition);
                // Destroy(_circleInstance);
                // _circleInstance = null;
                // _labelModeActive = false;
            }
        }
    }

    private void ShowLabelNameDialog()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _dialogInstance = Instantiate(_labelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);
        

        // InputField inputField = _dialogInstance.GetComponentInChildren<InputField>();

        // Button confirmBtn = _dialogInstance.transform.Find("ConfirmButton").GetComponent<Button>();
        // confirmBtn.onClick.AddListener(() =>
        // {
        //     string name = inputField.text;
        //     SaveLabel(_savedPosition, name);
        //     CleanupDialog();
        // });

        // Button cancelBtn = _dialogInstance.transform.Find("CancelButton").GetComponent<Button>();
        // cancelBtn.onClick.AddListener(() =>
        // {
        //     CleanupDialog();
        // });
    }

    private void SaveLabel(Vector3 position, string name)
    {
        Debug.Log("AI: Label saved at " + position + " with name " + name);
    }

    private void CleanupDialog()
    {
        _labelModeActive = false;
        if (_dialogInstance != null)
        {
            Destroy(_dialogInstance);
            _dialogInstance = null;
        }
        if (_circleInstance != null)
        {
            Destroy(_circleInstance);
            _circleInstance = null;
        }
    }    
}
