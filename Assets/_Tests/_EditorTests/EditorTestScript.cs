using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class EditorTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void DidAddyBegForUnitTests()
    {
        // Use the Assert class to test conditions
        string requesterName = "Addy";
        bool beggingForUnitTests = true;

        Assert.AreEqual("Addy", requesterName);
        Assert.IsTrue(beggingForUnitTests);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

}