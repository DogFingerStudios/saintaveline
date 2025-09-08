using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugInputManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerObject;
    private GameEntity playerEntity;
    
    void Awake()
    {
        playerEntity = _playerObject.GetComponent<GameEntity>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            playerEntity.Heal(5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            playerEntity.TakeDamage(5f);
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            SceneManager.LoadScene("Game");
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var playerInventory = player.GetComponent<CharacterInventory>();
            if (playerInventory == null) return;
            
            Debug.Log("Player Inventory Items:");
            foreach (var item in playerInventory.Items)
            {
                Debug.Log($"Item: {item.name}");
            }
        }
    }
}