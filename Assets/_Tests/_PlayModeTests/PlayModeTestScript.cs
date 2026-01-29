using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeTestScript
{
    [UnityTest]
    public IEnumerator MovePlayerToSpawnPosition()
    {
        var player = new GameObject("PlayerTest");
        var playerEntity = player.AddComponent<PlayerEntity>();

        var spawnTransform = new GameObject("PlayerSpawnTransformTest");
        playerEntity.SpawnLocation = spawnTransform.transform;
        playerEntity.SpawnLocation.position = new Vector3(-878f, 22f, -73f);

        Vector3 positionAfterSettingSpawnPoint = playerEntity.SetInitialPositionTest();

        yield return null;

        Assert.AreEqual(playerEntity.SpawnLocation, positionAfterSettingSpawnPoint);
    }

}