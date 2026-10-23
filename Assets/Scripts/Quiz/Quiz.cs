using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    // ── Interface interna para unificar SO e runtime ─────────────────
    private interface IQuestion
    {
        string GetQuestion();
        string GetAnswer(int index);
        int GetCorrectAnswerIndex();
    }

    private class SOQuestion : IQuestion
    {
        private QuestionSO _so;
        public SOQuestion(QuestionSO so) { _so = so; }
        public string GetQuestion() => _so.GetQuestion();
        public string GetAnswer(int i) => _so.GetAnswer(i);
        public int GetCorrectAnswerIndex() => _so.GetCorrectAnswerIndex();
    }

    private class RuntimeQuestion : IQuestion
    {
        private QuizRuntimeQuestion _rq;
        public RuntimeQuestion(QuizRuntimeQuestion rq) { _rq = rq; }
        public string GetQuestion() => _rq.GetQuestion();
        public string GetAnswer(int i) => _rq.GetAnswer(i);
        public int GetCorrectAnswerIndex() => _rq.GetCorrectAnswerIndex();
    }

    // ── Inspector ────────────────────────────────────────────────────
    [Header("Dados (fallback se não houver JSON)")]
    [SerializeField] private QuizData quizData;

    [Header("Timer")]
    [SerializeField] private Timer timer;

    [Header("UI - Pergunta")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("UI - Botões de resposta (4 botões na ordem 0-3)")]
    [SerializeField] private GameObject[] answerButtons;

    [Header("Sprites de feedback")]
    [SerializeField] private Sprite defaultAnswerSprite;
    [SerializeField] private Sprite correctAnswerSprite;
    [SerializeField] private Sprite wrongAnswerSprite;

    [Header("Delay antes de avançar para próxima pergunta (segundos)")]
    [SerializeField] private float delayBeforeNext = 1.2f;

    // ── Estado interno ───────────────────────────────────────────────
    private List<IQuestion> _questions = new List<IQuestion>();
    private int _currentIndex = 0;
    private bool _answered = false;
    private bool _runtimeInjected = false;

    // ── Contagem de acertos (exposta publicamente para o LevelCompletePanel) ──
    private int _correctAnswers = 0;
    public int CorrectAnswers => _correctAnswers;
    public int TotalQuestions => _questions.Count;

    // ── Injeção pelo QuizDataLoader ──────────────────────────────────
    public void InjectRuntimeQuestions(List<QuizRuntimeQuestion> runtimeList)
    {
        _questions.Clear();
        foreach (var rq in runtimeList)
            _questions.Add(new RuntimeQuestion(rq));
        _runtimeInjected = true;
    }

    // ── Unity ────────────────────────────────────────────────────────
    private void Start()
    {
        if (!_runtimeInjected)
            LoadFromScriptableObject();

        if (_questions.Count == 0)
        {
            Debug.LogError("[Quiz] Nenhuma pergunta disponível! Verifique QuizData ou o JSON.");
            return;
        }

        _correctAnswers = 0;
        timer.StartTimer();
        ShowQuestion(0);
    }

    private void LoadFromScriptableObject()
    {
        if (quizData == null || quizData.questions == null) return;
        foreach (var so in quizData.questions)
            _questions.Add(new SOQuestion(so));
    }

    // ── Exibição ─────────────────────────────────────────────────────
    private void ShowQuestion(int index)
    {
        if (index >= _questions.Count)
        {
            FinishQuiz();
            return;
        }

        _currentIndex = index;
        _answered = false;

        IQuestion q = _questions[index];
        questionText.text = q.GetQuestion();

        if (progressText != null)
            progressText.text = $"Pergunta {index + 1}/{_questions.Count}";

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI txt = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = q.GetAnswer(i);

            Image img = answerButtons[i].GetComponent<Image>();
            if (img != null && defaultAnswerSprite != null) img.sprite = defaultAnswerSprite;

            Button btn = answerButtons[i].GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }
    }

    // ── Input do jogador ─────────────────────────────────────────────
    public void OnAnswerSelected(int index)
    {
        if (_answered) return;
        _answered = true;

        SetButtonState(false);

        IQuestion q = _questions[_currentIndex];
        int correct = q.GetCorrectAnswerIndex();

        if (index == correct)
        {
            _correctAnswers++;
            SoundManager.Instance?.PlaySound2D("quiz_correct");
            SetButtonSprite(answerButtons[index], correctAnswerSprite);
            questionText.text = "Correto! ✓";
        }
        else
        {
            SoundManager.Instance?.PlaySound2D("quiz_wrong");
            SetButtonSprite(answerButtons[index], wrongAnswerSprite);
            SetButtonSprite(answerButtons[correct], correctAnswerSprite);
            questionText.text = $"Errado!\nResposta correta: {q.GetAnswer(correct)}";
        }

        StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeNext);
        ShowQuestion(_currentIndex + 1);
    }

    // ── Conclusão ────────────────────────────────────────────────────
    private void FinishQuiz()
    {
        timer.StopTimer();
        GameEvents.LevelCompleteMethod();
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private void SetButtonState(bool state)
    {
        foreach (var btn in answerButtons)
        {
            Button b = btn.GetComponent<Button>();
            if (b != null) b.interactable = state;
        }
    }

    private void SetButtonSprite(GameObject btnObj, Sprite sprite)
    {
        if (sprite == null || btnObj == null) return;
        Image img = btnObj.GetComponent<Image>();
        if (img != null) img.sprite = sprite;
    }
}