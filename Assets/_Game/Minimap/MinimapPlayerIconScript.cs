using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    [SerializeField] Camera _minimapCamera;

    void Update()
    {
        Vector3 newPosition = _minimapCamera.transform.position;
        newPosition.y -= 10;
        transform.position = newPosition;

        float newScale = _minimapCamera.orthographicSize * 0.125f;
        transform.localScale = new Vector3(newScale, newScale, newScale);
    }
}
