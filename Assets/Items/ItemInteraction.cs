using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class InteractionActionAttribute : Attribute
{
    public string ActionName { get; }

    public InteractionActionAttribute(string actionName)
    {
        ActionName = actionName;
    }
}

// This script is typically attached to an item in the game world to 
// allow the player to interact with it.
public class ItemInteraction : MonoBehaviour, Interactable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField] private ItemInteractMenu _menu;

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
        Debug.Log($"Action: {nameof(onTakeEquip)}");
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
        // nothing to do
    }

    void Update()
    {
        // nothing to do
    }
}
