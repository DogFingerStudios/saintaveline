using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image background;

    private Button button;
    private SaveFileListUI listOwner;
    private string saveFileName;

    private Color normalColor;
    private readonly Color selectedColor = new Color(0.25f, 0.45f, 0.85f, 1f);
 
    private void Awake()
    {
        button = GetComponent<Button>();

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (background != null)
        {
            normalColor = background.color;
        }

        button.onClick.AddListener(OnClicked);
    }

    public void Initialize(SaveFileListUI owner, string fileName)
    {
        listOwner = owner;
        saveFileName = fileName;

        if (label != null)
        {
            label.text = saveFileName;
        }

        SetSelected(false);
    }

    private void OnClicked()
    {
        if (listOwner != null)
        {
            listOwner.SelectRow(this, saveFileName);
        }
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
        {
            background.color = selected ? selectedColor : normalColor;
        }
    }
}