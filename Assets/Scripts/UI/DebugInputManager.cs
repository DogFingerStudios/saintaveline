using UnityEngine;

public class DebugInputManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            var iHasHealth = _playerObject.GetComponent<IHasHealth>();
            iHasHealth.Heal(5f);
        }
        
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            var iHasHealth = _playerObject.GetComponent<IHasHealth>();
            iHasHealth.TakeDamage(5f);
        }
    }
}
