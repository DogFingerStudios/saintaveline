using UnityEngine;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public void DoIt()
    {
        Debug.Log("DoIt called");
    }

    public void OpenMenu(List<string> interactions)
    {
        Debug.Log("OpenMenu called");
        foreach (var interaction in interactions)
        {
            Debug.Log($"Interaction: {interaction}");
        }
    }

    void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
