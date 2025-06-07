#nullable enable

using NUnit.Framework;
using UnityEngine;

// this class is attached to root NPC objects
public class EnemyNPC : BaseNPC
{
    [SerializeField, NPCStateDropdown]
    private string _defaultState = "EnemyIdle";

    public Transform[] PatrolPoints = new Transform[0];
    public float ArrivalThreshold = 0.5f;

    public float ViewAngle = 120f;
    public Vector3 EyeOffset = new(0f, 1.6f, 0f);
    
    private EnemyPatrolState? _patrolState = null;

    protected override void Start()
    {
        base.Start();

        var state = NPCStateFactory.CreateState(_defaultState, this);
        if (state != null) this.stateMachine.SetState(state);

        if (state is EnemyPatrolState patrolState)
        {
            _patrolState = patrolState;
        }
    }

    #region Unit Tests

    [Test]
    [Category("Enemy Unit Tests")]
    public void TestEnemyDamageDeath()
    {
        Health = 100f;

        TakeDamage(50f);

        Assert.AreEqual(50f, Health, "Health should be 50 after taking 50 damage.");

        // ::WARNING::
        // -----------
        // If we try to kill the enemy here, it will not have got it's references in time for this test
        // to pass. The solution would be to make this test a Coroutine and wait until the scene has loaded
        // and the enemy NPC has fetched all references. This test runs before all that.
        // The next problem, is that we need to use [UnityTest] to do that. Which means...
        // We need to use Assembly Definitions! Now this can be a pain, but if we do it, we need to
        // do it now before the project grown too large.
        // Within the Assembly Definition this script is under, we will need to add reference to the
        // UnityEngine.TestRunner and UnityEditor.TestRunner assemblies.

        // See folder "_UnitTests" and "UnitTestExample.cs" for an example of the Assembly Definition setup.
    }

    #endregion
}
