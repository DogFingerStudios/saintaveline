using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : CharacterEntity
{
    private Dictionary<string, Vector3> _labeledPoints = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> LabeledPoints { get => _labeledPoints; set => _labeledPoints = value; }

    void Update()
    {
        if (Health <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public override Vector3 DirectionToTarget(bool addFuzziness = false)
    {
        return Camera.main.transform.forward;
    }
}
