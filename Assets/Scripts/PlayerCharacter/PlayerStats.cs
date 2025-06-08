using System;
using System.Collections.Generic;
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

    private Dictionary<string, Vector3> _labeledPoints = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> LabeledPoints { get => _labeledPoints; set => _labeledPoints = value; }

    public event Action<float> OnHealthChanged;

    public float TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        this.OnHealthChanged?.Invoke(Health);
        return Health;
    }

    public float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        this.OnHealthChanged?.Invoke(Health);
        return Health;
    }

    public bool IsAlive => Health > 0;

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
