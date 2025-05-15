using System;
using System.Reflection;
using UnityEngine;
using System.Collections;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class InteractionActionAttribute : Attribute
{
    public string ActionName { get; }

    public InteractionActionAttribute(string actionName)
    {
        ActionName = actionName;
    }
}

/// <summary>
/// This script is attached to an item in the game world to
/// allow the player to interact with it.
/// </summary>
public class ItemInteraction : MonoBehaviour, Interactable
{
    [SerializeField] private ItemData _itemData;
    private EquippedItem _equippedItemScript;

    public string HelpText => $"Press [E] to interact with '{_itemData.ItemName}'";

    public void Interact()
    {
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OpenMenu(_itemData.Interactions);
    }

    private void DoInteraction(string actionName)
    {
        Type type = this.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                InteractionActionAttribute attr = method.GetCustomAttribute<InteractionActionAttribute>();
                if (attr != null && attr.ActionName == actionName)
                {
                    method.Invoke(this, null);
                    return;
                }
            }

            type = type.BaseType;
        }

        Debug.LogWarning($"No action found for '{actionName}' in {this.GetType().Name}");
    }

    [InteractionAction("take_equip")]
    protected virtual void onTakeEquip()
    {
        // get the `EquippedItemPos` GameObject that is a child of the `Player` GameObject
        // and set the position of the item to the position of the `EquippedItemPos` GameObject
        
        var player = GameObject.FindGameObjectWithTag("Player");
        var equippedItemPos = player.GetComponent<EquippedItem>();
        if (equippedItemPos == null)
        {
            Debug.LogWarning("EquippedItemPos not found in Player");
            return;
        }

        _equippedItemScript.EquippedItemObject = this.gameObject;
    }

    public virtual void onUnequipped()
    {
        // nothing to do
    }

    public void OnDefocus()
    {
        // nothing to do
    }

    public void OnFocus()
    {
        // nothing to do
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found");
            return;
        }
        _equippedItemScript = player.GetComponent<EquippedItem>();
        if (_equippedItemScript == null)
        {
            Debug.LogWarning("EquippedItem not found in Player");
            return;
        }
    }

    /// <summary>
    /// Code in this region is used as a default attack for items. This 
    /// has the player swing the item in a direction.
    /// </summary>
#region AttackCode

    private Vector3 _defaultLocalPosition;
    private Quaternion _defaultLocalRotation;
    private Coroutine? _swingCoroutine;

    public virtual void onEquipped()
    {
        _defaultLocalPosition = transform.localPosition;
        _defaultLocalRotation = transform.localRotation;
    }

    public virtual void Attack()
    {
        if (_swingCoroutine != null)
        {
            StopCoroutine(_swingCoroutine);
        }

        _swingCoroutine = StartCoroutine(AnimateSwing());
    }

    private IEnumerator AnimateSwing()
    {
        float duration = 0.25f;
        float elapsed = 0f;

        // AI: Define arc positions
        Vector3 rightStart = _defaultLocalPosition + new Vector3(0.25f, -0.05f, -0.1f);
        Vector3 leftEnd    = _defaultLocalPosition + new Vector3(-0.25f,  0.05f, -0.1f);

        // AI: Correct rotation for sideways twist (no forward thrust)
        Quaternion startRot = _defaultLocalRotation * Quaternion.Euler(0f, 45f, 15f);   // cocked back
        Quaternion endRot   = _defaultLocalRotation * Quaternion.Euler(0f, -45f, -15f); // full swing

        transform.localPosition = rightStart;
        transform.localRotation = startRot;

        yield return null;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(rightStart, leftEnd, t);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.localPosition = _defaultLocalPosition;
        transform.localRotation = _defaultLocalRotation;
        _swingCoroutine = null;
    }

#endregion AttackCode
  
}
