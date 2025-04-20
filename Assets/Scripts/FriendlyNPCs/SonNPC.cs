using UnityEngine;


public class SonNPC : FriendlyNPC
{   
    protected override void Start()
    {
        base.Start();

        var playerObject = GameObject.FindGameObjectWithTag("Player");
        Profile.Relationships.Add(playerObject, new RelationshipTraits
        {
            TrustToward = 0.99f,    // nearly full trust
            Love = 0.87f,           // nearly full love
            FearOf = 0.25f          // some fear
        });

        stateMachine.SetState(new NPCIdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
