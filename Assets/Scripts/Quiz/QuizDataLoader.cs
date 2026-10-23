using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Deve ser adicionado na cena do Quiz em um GameObject que execute ANTES do Quiz.cs
/// (use Script Execution Order: -100 ou coloque no mesmo objeto com ordem acima).
///
/// Lê o JSON salvo pela Área do Professor e popula uma lista de
/// QuizRuntimeQuestion que o Quiz.cs consumirá, sem precisar modificar
/// os ScriptableObjects em disco.
/// </summary>
public class QuizDataLoader : MonoBehaviour
{
    [Header("Referência ao Quiz (para injetar as perguntas em runtime)")]
    [SerializeField] private Quiz quiz;

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "quiz_questions.json");

    [System.Serializable]
    private class QuestionEntry
    {
        public string question;
        public string[] answers = new string[4];
        public int correctIndex;
    }

    [System.Serializable]
    private class QuizSaveData
    {
        public List<QuestionEntry> entries = new List<QuestionEntry>();
    }

    private void Awake()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        QuizSaveData saveData = JsonUtility.FromJson<QuizSaveData>(json);
        if (saveData == null || saveData.entries == null || saveData.entries.Count == 0) return;

        // Converte para QuizRuntimeQuestion e injeta no Quiz
        List<QuizRuntimeQuestion> runtimeQuestions = new List<QuizRuntimeQuestion>();
        foreach (var e in saveData.entries)
        {
            runtimeQuestions.Add(new QuizRuntimeQuestion
            {
                question = e.question,
                answers = e.answers,
                correctAnswerIndex = e.correctIndex
            });
        }

        quiz.InjectRuntimeQuestions(runtimeQuestions);
        Debug.Log($"[QuizDataLoader] {runtimeQuestions.Count} perguntas carregadas do JSON.");
    }
}

/// <summary>
/// Versão runtime de uma pergunta (não é ScriptableObject).
/// Usada quando as perguntas vêm do JSON da Área do Professor.
/// </summary>
[System.Serializable]
public class QuizRuntimeQuestion
{
    public string question;
    public string[] answers = new string[4];
    public int correctAnswerIndex;

    public string GetQuestion() => question;
    public string GetAnswer(int i) => (answers != null && i < answers.Length) ? answers[i] : "";
    public int GetCorrectAnswerIndex() => correctAnswerIndex;
}
