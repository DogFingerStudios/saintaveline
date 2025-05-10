using UnityEngine;
using UnityEngine.UI;

public class NPCInteractMenu : InteractMenuBase
{
    public Button stayButton;
    public Button followButton;
    private FriendlyNPC currentNPC;

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

        
            // currentNPC?.setState(new NPCFollowState(currentNPC));
            Close();
        });
    }
}
