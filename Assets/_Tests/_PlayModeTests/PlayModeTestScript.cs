using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeTestScript
{
    [UnityTest]
    public IEnumerator PlayerTakesDamage()
    {   
        GameObject player = null;
        try
        {
            player = new GameObject("PlayerTest");
            var playerEntity = player.AddComponent<PlayerEntity>();
            playerEntity.MaxHealth = 100;

            float dmgToTake = 50f;
            playerEntity.TakeDamage(dmgToTake);

            yield return null;

            Assert.AreEqual(playerEntity.MaxHealth - dmgToTake, playerEntity.Health);
        }
        finally
        {
            if (player != null) UnityEngine.Object.Destroy(player);
        }
    }

}