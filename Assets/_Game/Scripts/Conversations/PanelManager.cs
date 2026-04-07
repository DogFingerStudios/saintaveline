using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private AudioSource _audioSource;

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
        if (phrase == null)
        {
            _textField.text = "";
            return;
        }

        if (character == null || string.IsNullOrEmpty(character.Name))
        {
            _textField.text = phrase.GetText();
            return;
        }

        _textField.text = $"<color=#FFADAD>{character.Name}</color>: {phrase.GetText()}";
        if (_audioSource != null && phrase.GetAudio() != null)
        {
            _audioSource.clip = phrase.GetAudio();
            _audioSource.Play();
        }

    }

    public void SetText(PhrasingRef phrase)
    {
        _textField.text = phrase.GetText();
    }
}
