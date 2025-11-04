using System.Collections.Generic;
using UnityEngine;

public class CodexOverlayController : MonoBehaviour
{
    public static CodexOverlayController Instance { get; private set; }

    [SerializeField] private List<GameObject> TabContainers;
    [SerializeField] private Canvas CodexOverlayCanvas;

    public bool IsActive => CodexOverlayCanvas.enabled;


    private void Awake()
    {
        Instance = this;
    }

    public void ToggleCodexOverlay(bool? overrideState = null)
    {
        CodexOverlayCanvas.enabled = !CodexOverlayCanvas.enabled;

        if (overrideState.HasValue)
        {
            CodexOverlayCanvas.enabled = overrideState.Value;
        }

        if (CodexOverlayCanvas.enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SwitchTab(GameObject tabContainer)
    {
        foreach (var container in TabContainers)
        {
            if (container == tabContainer)
            {
                container.SetActive(true);
            }
            else
            {
                container.SetActive(false);
            }
        }
    }

    public void OpenCodexOverlay(CharacterEntity entity)
    {
        ToggleCodexOverlay(true);
    }
}