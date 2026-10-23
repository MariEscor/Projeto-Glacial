using System.Collections.Generic;
using UnityEngine;
using TMPro;  

public class SearchingWord : MonoBehaviour
{
    public TextMeshProUGUI displayedText;  
    public UnityEngine.UI.Image crossLine;

    private string _word;

    private void OnEnable()
    {
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    public void SetWord(string word)
    {
        _word = word;
        displayedText.text = word;
    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (word == _word)
        {
            crossLine.gameObject.SetActive(true);
        }
    }
}