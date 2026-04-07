using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private List<Button> _optionButtons = new List<Button>();

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

    public void SetOptions(List<DialogNodeSO> options)
    {
        if (options.Count > _optionButtons.Count)
        {
            throw new System.Exception("Not enough option buttons to display all options.");
        }

        for (int i = 0; i < _optionButtons.Count; i++) 
        { 
            if (i+1 > options.Count)
            {
                _optionButtons[i].gameObject.SetActive(false);
            }
            else
            {
                _optionButtons[i].gameObject.SetActive(true);
                _optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i].Title;
            }
        }
    }
}
