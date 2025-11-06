#nullable enable
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    // UI Elements
    public GameObject hudPanel;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI sonNPCStateText;
    public TextMeshProUGUI sonNPCDistanceText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI boatText;

    // Objects of interest
    public CharacterController controller;
    public GameObject enemyNPC;
    private GameEntity enemyNPCHealth;
    public SonNPC sonNPC;
    public BoatWaterDetector boatDetector;

    public GameObject distanceObject;

    private bool isVisible = true;
    private Transform _playerTransform;
    
    void Start()
    {
        if (enemyNPC)
        {
            enemyNPCHealth = enemyNPC.GetComponent<GameEntity>();
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Toggle visibility with "."
        if (Input.GetKeyDown(KeyCode.Period))
        {
            isVisible = !isVisible;
            hudPanel.SetActive(isVisible);
        }

        if (isVisible)
        {
            float distanceValue = Vector3.Distance(_playerTransform.position, distanceObject.transform.position);
            distanceText.text = "Distance: " + distanceValue.ToString("F2");


            //            sonNPCStateText.text = "SonNPC State: " + sonNPC.StateMachine.CurrentState?.GetType().Name;
            //            float distance = Vector3.Distance(controller.transform.position, sonNPC.transform.position);
            //            sonNPCDistanceText.text = "SonNPC Dist: " + distance.ToString("F2");

            //            // As we now have multiple enemies, we should either extend the DebugHUD to support multiple enemies
            //            // Or simply remove the enemy health display if not needed
            //            // This object check prevents null reference exceptions when we don't have an enemy assigned
            //            if (enemyNPC != null && enemyNPCHealth != null)
            //            {
            //                enemyHealthText.text = "Enemy Health: " + enemyNPCHealth.Health.ToString("F2");
            //            }

            //            string boatLandTest = boatDetector.IsOverland ? "Overland" : "Not Overland";
            //            string boatWaterTest = boatDetector.IsOnWater ? "On Water" : "Not On Water";
            //            string boatBeachedTest = boatDetector.IsBeached ? "Beached" : "Not Beached";

            //            boatText.text = $@"Boat Land: {boatLandTest}
            //Boat Water: {boatWaterTest}
            //Boat Beached: {boatBeachedTest}
            //AvgWaterDepth: {boatDetector.AvgWaterDepth}
            //WaterCoverage01: {boatDetector.WaterCoverage01}
            //MinGroundClearance: {boatDetector.MinGroundClearance}
            //WaterHits: {boatDetector.WaterHits}
            //SamplePoints:
            //{SamplePointsString()}
            //";
        }
    }

    string SamplePointsString()
    {
        if (boatDetector.SamplePoints == null) return string.Empty;
        string s = "";
        for (int i = 0; i < boatDetector.SamplePoints.Length; i++)
        {
            if (boatDetector.SamplePoints[i] != null)
            {
                s += boatDetector.SamplePoints[i].name + ":" + boatDetector.SamplePoints[i].position.ToString("F2") + "\n";
            }
        }
        return s;
    }

}