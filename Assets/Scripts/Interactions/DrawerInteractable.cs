using UnityEngine;

public class DrawerInteractable : Interactable
{
    public override void Interact()
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
}
