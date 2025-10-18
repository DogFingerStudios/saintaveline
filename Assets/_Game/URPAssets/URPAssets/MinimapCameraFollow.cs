using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;

    void Update()
    {
        Vector3 newPosition = _target.transform.position;
        newPosition.y = 100;
        this.transform.position = newPosition;

        Vector3 newRotation = this.transform.eulerAngles;
        newRotation.y = _target.transform.eulerAngles.y;
        this.transform.eulerAngles = newRotation;
    }
}
