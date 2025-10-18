using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _camera;

    private float _minSize = 10f;
    private float _maxSize = 5000f;

    private void Awake()
    {
    }

    void Update()
    {
        Vector3 newPosition = _target.transform.position;
        newPosition.y = 100f;
        this.transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Minus) && _camera.orthographicSize < _maxSize)
        {
            //newPosition += new Vector3(0, 100, 0);
            _camera.orthographicSize += 7.5f;
        }
        else if (Input.GetKeyDown(KeyCode.Equals) && _camera.orthographicSize > _minSize)
        {
            //newPosition -= new Vector3(0, 100, 0);
            _camera.orthographicSize -= 7.5f;
        }

        //Vector3 newRotation = this.transform.eulerAngles;
        //newRotation.y = _target.transform.eulerAngles.y;
        //this.transform.eulerAngles = newRotation;
    }
}
