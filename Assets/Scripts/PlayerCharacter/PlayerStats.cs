using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour, IHasHealth
{
    [SerializeField]
    float _health = 100f;

    public float Health 
    {
        get => _health;
        set => _health = value;
    }

    [SerializeField]
    float _maxHealth = 100f;
    public float MaxHealth 
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    public float TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        return Health;
    }

    public float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        return Health;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (Health <= 0)
        {
            SceneManager.LoadScene("MainMenu"); 
        }
    }
}
