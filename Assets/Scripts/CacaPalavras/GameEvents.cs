using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public delegate void EnableSquareSelection();
    public static event EnableSquareSelection OnEnableSquareSelection;
    public static void EnableSquareSelectionMethod()
    {
        if (OnEnableSquareSelection != null) OnEnableSquareSelection();
    }

    public delegate void DisableSquareSelection();
    public static event DisableSquareSelection OnDisableSquareSelection;
    public static void DisableSquareSelectionMethod()
    {
        if (OnDisableSquareSelection != null) OnDisableSquareSelection();
    }

    public delegate void SelectSquare(Vector3 position);
    public static event SelectSquare OnSelectSquare;
    public static void SelectSquareMethod(Vector3 position)
    {
        if (OnSelectSquare != null) OnSelectSquare(position);
    }

    public delegate void CheckSquare(string letter, Vector3 squarePosition, int squareIndex);
    public static event CheckSquare OnCheckSquare;
    public static void CheckSquareMethod(string letter, Vector3 squarePosition, int squareIndex)
    {
        if (OnCheckSquare != null) OnCheckSquare(letter, squarePosition, squareIndex);
    }

    public delegate void ClearSelection();
    public static event ClearSelection OnClearSelection;
    public static void ClearSelectionMethod()
    {
        if (OnClearSelection != null) OnClearSelection();
    }

    public delegate void CorrectWord(string word, List<int> squareIndexes);
    public static event CorrectWord OnCorrectWord;
    public static void CorrectWordMethod(string word, List<int> squareIndexes)
    {
        if (OnCorrectWord != null) OnCorrectWord(word, squareIndexes);
    }

    public delegate void LevelComplete();
    public static event LevelComplete OnLevelComplete;
    public static void LevelCompleteMethod()
    {
        if (OnLevelComplete != null) OnLevelComplete();
    }

    public delegate void TimesUp();
    public static event TimesUp OnTimesUp;
    public static void TimesUpMethod()
    {
        if (OnTimesUp != null) OnTimesUp();
    }

    public delegate void GameOver();
    public static event GameOver OnGameOver;
    public static void GameOverMethod()
    {
        if (OnGameOver != null) OnGameOver();
    }

    // disparado quando uma chave de progresso é desbloqueada
    // os PathBlockers escutam este evento para sumir imediatamente
    public delegate void PathUnlocked(string progressKey);
    public static event PathUnlocked OnPathUnlocked;
    public static void PathUnlockedMethod(string progressKey)
    {
        if (OnPathUnlocked != null) OnPathUnlocked(progressKey);
    }
}