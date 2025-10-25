#nullable enable

using UnityEngine;

// public class DrawerInteractable : MonoBehaviour, ItemInteractable
public class DrawerInteractable : ItemEntity
{
    public override string HoverText
    {
        get { return "Press [E] to open/close drawer"; }
    }

    public override void Interact(GameEntity? interactor = null)
    {
        // print the name of the object being interacted with
        // Debug.Log("Drawer " + gameObject.name + " opened!");

        DrawerMech drawer = GetComponent<DrawerMech>();
        if (drawer == null) return;

        // toggle the drawer's state
        drawer.drawerBool = !drawer.drawerBool;
    }

    public override void OnFocus()
    {
        // print the name of the object being focused on
        // Debug.Log("Looking at drawer " + gameObject.name);
    }

    public override void OnDefocus()
    {
        // print the name of the object being defocused from
        // Debug.Log("Defocused from drawer " + gameObject.name);
    }
}
