using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que armazena a lista de perguntas do Quiz.
/// Crie via: Assets > Create > Quiz > QuizData
/// </summary>
[CreateAssetMenu(menuName = "Quiz/QuizData", fileName = "QuizData")]
public class QuizData : ScriptableObject
{
    public List<QuestionSO> questions = new List<QuestionSO>();
}
