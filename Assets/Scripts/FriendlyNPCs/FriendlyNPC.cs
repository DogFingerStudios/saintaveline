using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class FriendlyNPC : BaseNPC, Interactable
{
    // TODO: these three fields should be refactored out of here
    [SerializeField] private GameObject _mapLabelDialogPrefab;
    [SerializeField] private Canvas _uiCanvas;
    private PlayerStats _playerStats;


    public List<InteractionData> Interactions = new List<InteractionData>();

    protected override void Start()
    {
        base.Start();
        Interactions.Add(new InteractionData { key = "stay", description = "Stay" });
        Interactions.Add(new InteractionData { key = "follow", description = "Follow" });
        Interactions.Add(new InteractionData { key = "goto", description = "Go To2" });

        _playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (_playerStats == null)
        {
            // TODO: error handling is shyte!
            Debug.LogError("PlayerStats component not found on the player character.");
        }
    }

    #region Interactable Interface Implementation
    public string HelpText => "Press [E] to interact";

    public void OnFocus()
    {
        // Optional: highlight outline, play sound, etc.
    }

    public void OnDefocus()
    {
        // Cleanup when not hovered
    }

    public void Interact()
    {
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OpenMenu(Interactions);
    }
    #endregion

    // TODO: this is copied from ItemInteraction.cs, should be refactored to a common base class
    private void DoInteraction(string actionName)
    {
        Type type = this.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                InteractionActionAttribute attr = method.GetCustomAttribute<InteractionActionAttribute>();
                if (attr != null && attr.ActionName == actionName)
                {
                    method.Invoke(this, null);
                    return;
                }
            }

            type = type.BaseType;
        }

        Debug.LogWarning($"No action found for '{actionName}' in {this.GetType().Name}");
    }

    [InteractionAction("stay")]
    protected virtual void onStay()
    {
        this.setState(new NPCIdleState(this));
    }

    [InteractionAction("follow")]
    protected virtual void onFollow()
    {
        this.Panic();

        var entityTraits = this.Profile.Personality;
        var relationshipTraits = this.Profile.Relationships[GameObject.FindGameObjectWithTag("Player")];
        if (DecisionProfile.Evaluate(1, 1, entityTraits, relationshipTraits) == DecisionResult.Obey)
        {
            this.setState(new NPCFollowState(this));
        }
        else
        {
            this.setState(new NPCIdleState(this));
        }
    }

    [InteractionAction("goto")]
    protected virtual void onGoTo()
    {
        InteractionManager.Instance.OnLateInteractionAction += ResetCursor;

        var dlgInstance = Instantiate(_mapLabelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);
        Button confirmBtn = dlgInstance.transform.Find("ButtonContainer/ConfirmButton").GetComponent<Button>();
        confirmBtn.onClick.AddListener(() =>
        {
            var labelDropdown = dlgInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
            if (labelDropdown.value == 0) return;

            string labelName = labelDropdown.options[labelDropdown.value].text;
            var destination = _playerStats.LabeledPoints[labelName];
            this.setState(new NPCGoToState(this, destination));

            Destroy(dlgInstance);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });

        Button cancelBtn = dlgInstance.transform.Find("ButtonContainer/CancelButton").GetComponent<Button>();
        cancelBtn.onClick.AddListener(() =>
        {
            Destroy(dlgInstance);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });

        var labelDropdown = dlgInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
        labelDropdown.ClearOptions();
        var options = new List<string>(_playerStats.LabeledPoints.Keys);
        options.Insert(0, "Select a label");
        labelDropdown.AddOptions(options);
    }

    private void ResetCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        InteractionManager.Instance.OnLateInteractionAction -= ResetCursor;
    }
}

