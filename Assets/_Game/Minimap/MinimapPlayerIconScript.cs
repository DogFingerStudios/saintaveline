using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    private Transform _playerTransform;

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        
    }
}
