using UnityEngine;


public class SonNPC : FriendlyNPC
{   
    private void Start()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        Relationships.Add(playerObject, new RelationshipTraits
        {
            TrustToward = 0.99, // nearly full trust
            Love = 0.87f,       // nearly full love
            FearOf = 0.25f      // some fear
        });

        stateMachine.SetState(new NPCIdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
