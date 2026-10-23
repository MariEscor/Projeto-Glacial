using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }

    private AlphabetData.LetterData _normalLetterData;
    private AlphabetData.LetterData _selectedLetterData;
    private AlphabetData.LetterData _correctLetterData;

    private SpriteRenderer _displayedImage;

    private bool _selected;
    private bool _clicked;
    private int _index = -1;
    private bool _correct;

    // Estado compartilhado entre todos os GridSquares
    private static Vector3 _originPos = Vector3.zero;
    private static Vector3 _lockedDir = Vector3.zero;
    private static float _stepSize = 0f;
    private static int _lockedCount = 0;

    public void SetIndex(int index) => _index = index;
    public int GetIndex() => _index;

    void Start()
    {
        _selected = false;
        _clicked = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable()
    {
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    private void CorrectWord(string word, List<int> squareIndexes)
    {
        if (_selected && squareIndexes.Contains(_index))
        {
            _correct = true;
            _displayedImage.sprite = _correctLetterData.image;
        }

        _selected = false;
        _clicked = false;
        ResetStatic();
    }

    public void OnEnableSquareSelection()
    {
        _clicked = true;
    }

    public void OnDisableSquareSelection()
    {
        _selected = false;
        _clicked = false;
        ResetStatic();

        if (_correct)
            _displayedImage.sprite = _correctLetterData.image;
        else
            _displayedImage.sprite = _normalLetterData.image;
    }

    private void SelectSquare(Vector3 position)
    {
        if (this.gameObject.transform.position == position)
            _displayedImage.sprite = _selectedLetterData.image;
    }

    public void SetSprite(AlphabetData.LetterData normalLetterData,
                          AlphabetData.LetterData selectedLetterData,
                          AlphabetData.LetterData correctLetterData)
    {
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;
        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }

    private void OnMouseDown()
    {
        ResetStatic();
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.image;
    }

    private void OnMouseEnter()
    {
        if (!_clicked) return;

        // Direção ainda não travada: 2º quadrado, comportamento original
        if (_lockedDir == Vector3.zero)
        {
            CheckSquare();
            return;
        }

        // Direção travada: projeta posição do mouse na linha e preenche todos os quadrados
        // entre o último selecionado e onde o mouse está agora
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        float projDist = Vector3.Dot(mouseWorld - _originPos, _lockedDir);
        int targetStep = Mathf.Max(1, Mathf.RoundToInt(projDist / _stepSize));

        // Preenche do próximo passo esperado até targetStep
        for (int step = _lockedCount; step <= targetStep; step++)
        {
            Vector3 pos = _originPos + _lockedDir * _stepSize * step;
            GridSquare sq = FindSquareAt(pos);
            if (sq != null && !sq._selected)
            {
                sq._clicked = true; // garante que CheckSquare vai funcionar
                sq.CheckSquare();
            }
        }
    }

    private void OnMouseUp()
    {
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    public void CheckSquare()
    {
        if (_selected == false && _clicked == true)
        {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);

            // 1º quadrado selecionado: registra origem
            if (_lockedCount == 0)
            {
                _originPos = transform.position;
                _lockedCount = 1;
            }
            // 2º quadrado: calcula e trava a direção
            else if (_lockedDir == Vector3.zero)
            {
                Vector3 d = transform.position - _originPos;
                _stepSize = d.magnitude;
                Vector3 dn = d.normalized;
                _lockedDir = new Vector3(
                    Mathf.Round(dn.x),
                    Mathf.Round(dn.y),
                    0f).normalized;
                _lockedCount = 2;
            }
            else
            {
                _lockedCount++;
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────

    private static GridSquare FindSquareAt(Vector3 pos, float tol = 0.15f)
    {
        // Busca por todos os GridSquares na cena e acha o mais próximo
        GridSquare[] all = Object.FindObjectsByType<GridSquare>(FindObjectsSortMode.None);
        foreach (var sq in all)
            if (Vector3.Distance(sq.transform.position, pos) < tol)
                return sq;
        return null;
    }

    private static void ResetStatic()
    {
        _lockedDir = Vector3.zero;
        _originPos = Vector3.zero;
        _stepSize = 0f;
        _lockedCount = 0;
    }
}