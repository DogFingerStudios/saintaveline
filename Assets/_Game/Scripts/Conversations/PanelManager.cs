using TMPro;
using UnityEngine;
using UnityEngine.Video;

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
    public void SetText(CharacterEntity character, PhrasingRef phrase)
    {
        var text = $"<color=#FF0000>{character.name}</color>: {phrase.GetText()}";
        _textField.text = text;
    }

    public void SetText(PhrasingRef phrase)
    {
        _textField.text = phrase.GetText();
    }
}
