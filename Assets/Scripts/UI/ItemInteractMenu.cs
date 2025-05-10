using UnityEngine;
using UnityEngine.UI;

public class ItemInteractMenu : InteractMenuBase
{
    public Button kickButton;
    public Button pickUpButton;

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void Start()
    {
        base.Start();

        kickButton.onClick.AddListener(() =>
        {
            Debug.Log("Kick button clicked");
            Close();
        });

        pickUpButton.onClick.AddListener(() =>
        {
            Debug.Log("Pick up button clicked");
            Close();
        });
    }
}
