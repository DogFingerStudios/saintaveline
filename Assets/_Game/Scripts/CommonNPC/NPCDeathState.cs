#nullable enable

using UnityEngine;
using System.Collections;

public class NPCDeathState : NPCState
{
    public NPCDeathState(BaseNPC baseNpc) : base(baseNpc)
    {
        // nothing to do
    }

    public override void Enter()
    {
        Debug.Log($"{this.NPC!.name} has died.");
        Rigidbody rb = this.NPC!.GetComponent<Rigidbody>();
        if  (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None; 
            rb.linearDamping = 2f;  
            rb.angularDamping = 1f;  
            rb.AddTorque(Vector3.right * 5f, ForceMode.Impulse);
        }

        UnityEngine.AI.NavMeshAgent navAgent = this.NPC!.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
    }

    public override void Exit()
    {
        // Nothing to do in the death state
    }

    private float _delayBeforeFade = 10f;   // AI: Seconds to wait before starting fade
    private float _stateTimer = 0f;         // AI: Tracks time in this state
    private bool _fadeStarted = false;      // AI: Ensures fade starts once
    private float _fadeDuration = 2f;       // AI: Fade duration

    public override NPCStateReturnValue? Update()
    {
        _stateTimer += Time.deltaTime;                                 // AI: Accumulate time

        if (!_fadeStarted && _stateTimer >= _delayBeforeFade)
        {
            _fadeStarted = true;                                      // AI: Prevent multiple starts
            this.NPC!.StartCoroutine(FadeOutAndDestroy());
        }

        return null;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        var renderer = this.NPC!.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            yield return new WaitForSeconds(_fadeDuration);
            UnityEngine.Object.Destroy(NPC.gameObject);
            yield break;
        }

        var material = renderer.material;
        var originalColor = material.color;

        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;

        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime; // AI: Accumulate fade time
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / _fadeDuration);
            var c = originalColor;
            c.a = alpha;
            material.color = c;
            yield return null;
        }

        UnityEngine.Object.Destroy(NPC.gameObject);
    }
}
