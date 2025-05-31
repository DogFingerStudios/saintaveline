using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPCInteractMenu : InteractMenuBase
{
    public Button stayButton;
    public Button followButton;
    public Button goToButton;
    private FriendlyNPC currentNPC;

    private GameObject _dialogInstance;
    [SerializeField] private GameObject _mapLabelDialogPrefab;
    [SerializeField] private Canvas _uiCanvas;
    private PlayerStats _playerStats;

    public void Open(FriendlyNPC npc)
    {
        base.Open();
        currentNPC = npc;
    }

    public override void Close()
    {
        base.Close();
        currentNPC = null;
    }

    protected override void Start()
    {
        _playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (_playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on the player character.");
            return;
        }

        base.Start();

        stayButton.onClick.AddListener(() =>
        {
            currentNPC?.setState(new NPCIdleState(currentNPC));
            Close();
        });

        followButton.onClick.AddListener(() =>
        {
            Debug.Log("Follow button clicked");
            currentNPC?.Panic();

            var entityTraits = currentNPC?.Profile.Personality;
            var relationshipTraits = currentNPC?.Profile.Relationships[GameObject.FindGameObjectWithTag("Player")];
            if (DecisionProfile.Evaluate(1, 1, entityTraits, relationshipTraits) == DecisionResult.Obey)
            {
                currentNPC?.setState(new NPCFollowState(currentNPC));
            }
            else
            {
                currentNPC?.setState(new NPCIdleState(currentNPC));
            }

            Close();
        });
        
        goToButton.onClick.AddListener(() =>
        {
            Debug.Log("GoTo button clicked");

            // helpText.SetActive(true);
            // crossHair.SetActive(true);
            panel.SetActive(false);
            // Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Locked;

            _dialogInstance = Instantiate(_mapLabelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);
            Button confirmBtn = _dialogInstance.transform.Find("ButtonContainer/ConfirmButton").GetComponent<Button>();
            confirmBtn.onClick.AddListener(() =>
            {
                var labelDropdown = _dialogInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
                string labelName = labelDropdown.options[labelDropdown.value].text;
                if (labelName == "Select a label") return;
                var destination = _playerStats.LabeledPoints[labelName];
                Debug.Log($"GoTo button clicked with label '{labelName}' at position {destination}");

                var agent = currentNPC!.GetComponent<NavMeshAgent>();
                agent.SetDestination(destination);

                Close();
                Destroy(_dialogInstance);
            });

            Button cancelBtn = _dialogInstance.transform.Find("ButtonContainer/CancelButton").GetComponent<Button>();
            cancelBtn.onClick.AddListener(() =>
            {
                Close();
                Destroy(_dialogInstance);
            });

            var labelDropdown = _dialogInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
            labelDropdown.ClearOptions();
            var options = new List<string>(_playerStats.LabeledPoints.Keys);
            options.Insert(0, "Select a label");
            labelDropdown.AddOptions(options);
        });
    }
}
