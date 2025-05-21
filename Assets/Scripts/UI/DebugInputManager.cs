using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugInputManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerObject;
    private IHasHealth iHasHealth;

    
    void Awake()
    {
        iHasHealth = _playerObject.GetComponent<IHasHealth>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            iHasHealth.Heal(5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            iHasHealth.TakeDamage(5f);
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            SceneManager.LoadScene("Game"); 
        }
    }
}