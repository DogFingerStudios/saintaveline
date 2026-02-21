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

    [Test]
    public void ThisWillFail()
    {
        // AI: This test is intentionally designed to fail to demonstrate the testing framework's ability to catch failures.
        int expectedValue = 42;
        int actualValue = 24; // This value is incorrect on purpose

        Assert.AreEqual(expectedValue, actualValue, "The actual value does not match the expected value, indicating a failure in the test.");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // AI: Use the Assert class to test conditions by verifying simple state changes.
        int _frameCount = 0;
        _frameCount++;
        Assert.AreEqual(1, _frameCount);

        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

}