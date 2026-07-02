using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private SaveFileRowUI rowPrefab;
    [SerializeField] private Button playButton;

    private SaveFileRowUI selectedRow;
    private string selectedSaveFileName = string.Empty;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }

    private void Start()
    {
        PopulateList(new List<string>
        {
            "Save001.json",
            "Save002.json",
            "VillageStart.json"
        });
    }

    public void PopulateList(List<string> saveFiles)
    {
        ClearList();

        selectedRow = null;
        selectedSaveFileName = string.Empty;

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        foreach (string saveFile in saveFiles)
        {
            SaveFileRowUI row = Instantiate(rowPrefab, contentParent);
            row.Initialize(this, saveFile);
        }
    }

    public void SelectRow(SaveFileRowUI row, string saveFileName)
    {
        if (selectedRow != null)
        {
            selectedRow.SetSelected(false);
        }

        selectedRow = row;
        selectedSaveFileName = saveFileName;

        selectedRow.SetSelected(true);

        if (playButton != null)
        {
            playButton.interactable = true;
        }

        Debug.Log("Selected save file: " + selectedSaveFileName);
    }

    public string GetSelectedSaveFileName()
    {
        return selectedSaveFileName;
    }

    private void ClearList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}