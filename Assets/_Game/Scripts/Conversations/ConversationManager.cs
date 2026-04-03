using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
