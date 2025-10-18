using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;

    void Update()
    {
        Vector3 newPosition = _target.transform.position;
        newPosition.y = 100;
        this.transform.position = newPosition;        
    }
}
