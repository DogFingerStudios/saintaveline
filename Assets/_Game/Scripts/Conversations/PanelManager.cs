using TMPro;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _textField;

    public void EnableAll()
    {
        _panel.SetActive(true);
        _textField.enabled = true;
    }

    public void DisableAll()
    {
        _panel.SetActive(false);
        _textField.enabled = false;
    }
    public void SetText(string text)
    {
        _textField.text = text;
    }

}
