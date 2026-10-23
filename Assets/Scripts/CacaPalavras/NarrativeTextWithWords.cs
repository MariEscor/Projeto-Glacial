using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class NarrativeTextWithWords : MonoBehaviour
{
    [TextArea(5, 15)]
    public string narrativeText;

    public Color highlightColor     = new Color(0.18f, 0.52f, 0.89f, 0.85f); // badge azul
    public Color highlightTextColor = new Color(1f,    1f,    1f,    1f);     // texto branco
    public Color foundTextColor     = new Color(0.4f,  0.4f,  0.4f,  1f);    // cinza riscado

    public TextMeshProUGUI textDisplay;

    private string _baseNarrative = "";
    private HashSet<string> _foundWords = new HashSet<string>();
    private List<string> _detectedDisplayWords = new List<string>();

    private void Start()
    {
        _baseNarrative = narrativeText;
        BuildText();
    }

    private string Normalize(string s)
    {
        s = s.Replace(" ", "").ToUpper();
        s = s.Replace("Á","A").Replace("À","A").Replace("Â","A").Replace("Ã","A")
             .Replace("É","E").Replace("Ê","E")
             .Replace("Í","I").Replace("Î","I")
             .Replace("Ó","O").Replace("Ô","O").Replace("Õ","O")
             .Replace("Ú","U").Replace("Û","U")
             .Replace("Ç","C");
        return s;
    }

    private void BuildText()
    {
        _detectedDisplayWords.Clear();

        var matches = Regex.Matches(_baseNarrative,
            @"\b[A-ZÁÉÍÓÚÂÊÎÔÛÃÕÇ]{2,}(?:_[A-ZÁÉÍÓÚÂÊÎÔÛÃÕÇ]{2,})*\b");

        var seen = new HashSet<string>();
        var matchList = new List<Match>();
        foreach (Match m in matches) matchList.Add(m);

        for (int i = matchList.Count - 1; i >= 0; i--)
        {
            string raw     = matchList[i].Value;
            string display = raw.Replace("_", " ");
            if (!seen.Contains(raw))
            {
                seen.Add(raw);
                _detectedDisplayWords.Insert(0, display);
            }
        }

        textDisplay.text = BuildTaggedText(_baseNarrative);
    }

    private string BuildTaggedText(string source)
    {
        string hexHL     = ColorUtility.ToHtmlStringRGBA(highlightColor);
        string hexHLText = ColorUtility.ToHtmlStringRGBA(highlightTextColor);
        string hexFound  = ColorUtility.ToHtmlStringRGBA(foundTextColor);

        var matches = Regex.Matches(source,
            @"\b[A-ZÁÉÍÓÚÂÊÎÔÛÃÕÇ]{2,}(?:_[A-ZÁÉÍÓÚÂÊÎÔÛÃÕÇ]{2,})*\b");

        var matchList = new List<Match>();
        foreach (Match m in matches) matchList.Add(m);

        string result = source;

        for (int i = matchList.Count - 1; i >= 0; i--)
        {
            string raw     = matchList[i].Value;
            string display = raw.Replace("_", " ");

            string tagged;
            if (_foundWords.Contains(display))
            {
                // sem fundo, só texto cinza riscado
                tagged = $"<color=#{hexFound}><s>{display}</s></color>";
            }
            else
            {
                // badge azul com texto branco em negrito
                tagged = $"<mark=#{hexHL}> <color=#{hexHLText}><b>{display}</b></color> </mark>";
            }

            result = result.Substring(0, matchList[i].Index)
                   + tagged
                   + result.Substring(matchList[i].Index + raw.Length);
        }

        return result;
    }

    private void OnEnable()  => GameEvents.OnCorrectWord += OnCorrectWord;
    private void OnDisable() => GameEvents.OnCorrectWord -= OnCorrectWord;

    private void OnCorrectWord(string word, List<int> indexes)
    {
        string wordNorm = Normalize(word);
        foreach (var display in _detectedDisplayWords)
        {
            if (Normalize(display) == wordNorm)
            {
                _foundWords.Add(display);
                textDisplay.text = BuildTaggedText(_baseNarrative);
                return;
            }
        }
    }
}