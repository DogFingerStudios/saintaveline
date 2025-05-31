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
        _playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (_playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on the player character.");
            return;
        }

        if (_playerStats.LabeledPoints.Count == 0)
        {
            goToButton.interactable = false;
        }
        else
        {
            goToButton.interactable = true;
        }

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
            panel.SetActive(false);

            _dialogInstance = Instantiate(_mapLabelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);
            Button confirmBtn = _dialogInstance.transform.Find("ButtonContainer/ConfirmButton").GetComponent<Button>();
            confirmBtn.onClick.AddListener(() =>
            {
                var labelDropdown = _dialogInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
                string labelName = labelDropdown.options[labelDropdown.value].text;
                if (labelName == "Select a label") return;
                var destination = _playerStats.LabeledPoints[labelName];
                currentNPC?.setState(new NPCGoToState(currentNPC, destination));

                Destroy(_dialogInstance);
                Close();
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
