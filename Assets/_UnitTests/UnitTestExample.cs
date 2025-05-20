using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UnitTestExample : MonoBehaviour
{

    /// <summary>
    /// [Test] is a standard NUnity test attribute.
    /// </summary>
    [Test]
    [Category("Example Unit Tests")]
    public void TestSomeCalculation()
    {
        // Set up the thing
        int a = 5;
        int b = 100;

        // Do the thing
        int result = a * (b + 100) - 400;

        // Check the thing
        Assert.AreEqual(600, result, "5 * (100 + 100) - 400 should equal 600.");
    }

    /// <summary>
    /// [UnityTest] is a Unity test attribute that allows for coroutines.
    /// This requires using UnityEngine.TestTools, which also requires Assembly References
    /// to UnityEditor.TestRunner and UnityEngine.TestRunner.
    /// </summary>
    [UnityTest]
    [Category("Example Unit Tests")]
    public IEnumerator TestWaitForCalculation()
    {
        // Set up the thing
        int a = 5;
        int b = 100;

        // Wait for a frame (or more)
        yield return new WaitForSecondsRealtime(1); // Wait for 1 second

        // Do the thing
        int result = a * (b + 100) - 400;

        // Check the thing
        Assert.AreEqual(600, result, "5 * (100 + 100) - 400 should equal 600.");
    }

}