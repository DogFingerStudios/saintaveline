using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveFileListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private SaveFileRowUI rowPrefab;
    [SerializeField] private Button playButton;

    private SaveFileRowUI _selectedRow;
    private string _selectedSaveFileName = string.Empty;
    private string _saveDirectoryPath = null!;

    private void Awake()
    {
        _saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(_saveDirectoryPath))
        {
            Directory.CreateDirectory(_saveDirectoryPath);
        }

        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }

    private void Start()
    {
        string[] saveFiles = Directory.GetFiles(_saveDirectoryPath, "*.json");
        PopulateList(new List<string>(saveFiles));
    }

    public void PopulateList(List<string> saveFiles)
    {
        ClearList();

        _selectedRow = null;
        _selectedSaveFileName = string.Empty;

        if (playButton != null)
        {
            playButton.interactable = false;
        }

        foreach (string saveFile in saveFiles)
        {
            SaveFileRowUI row = Instantiate(rowPrefab, contentParent);
            row.Initialize(this, Path.GetFileName(saveFile));
        }
    }

    public void SelectRow(SaveFileRowUI row, string saveFileName)
    {
        if (_selectedRow != null)
        {
            _selectedRow.SetSelected(false);
        }

        _selectedRow = row;
        _selectedSaveFileName = saveFileName;

        _selectedRow.SetSelected(true);

        if (playButton != null)
        {
            playButton.interactable = true;
        }

        Debug.Log("Selected save file: " + _selectedSaveFileName);
    }

    public string GetSelectedSaveFileName()
    {
        return Path.Combine(_saveDirectoryPath, _selectedSaveFileName);
    }

    private void ClearList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}