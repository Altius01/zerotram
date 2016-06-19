﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NewCharacterWindow : MonoBehaviour
{
    [SerializeField] private Text _description;
    [SerializeField] private List<Sprite> _images;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _window;
    
    public Dictionary<string, int> CharacterIndices = new Dictionary<string, int>()
    {
        {"gnome", 0},
        {"granny", 1},
        {"cat", 2},
        {"alien", 3},
        {"bird", 4}
    }; 
    
    private string _currentCharacter;

    public const string Prefix = "CharacterWindow";

    public void OnExit()
    {
        Time.timeScale = 1;
        _window.SetActive(false);
    }
    
    public void SetCharacterToShow(string character)
    {
        if(_window == null)
            return;
        if(_window.activeSelf)
            return;
        _currentCharacter = character;
        ShowCharacter();
    }

    private void ShowCharacter()
    {
        Time.timeScale = 0;
        int imageIndex = -1;
        CharacterIndices.TryGetValue(_currentCharacter, out imageIndex);
        if (imageIndex >= 0)
        {
            _image.sprite = _images[imageIndex];
            _image.SetNativeSize();
            _description.text = StringResources.GetLocalizedString(_currentCharacter);
            _window.SetActive(true);
        }
    }
}
