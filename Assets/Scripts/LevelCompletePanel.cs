using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCompletePanel : MonoBehaviour
{
    [Header("Referências")]
    public Timer timer;
    public GameData currentGameData;

    [Header("Objetos para esconder ao mostrar painel")]
    public GameObject[] objectsToHide;

    [Header("Modo de estrelas")]
    public StarMode starMode = StarMode.ByCorrectAnswers;
    public GameManager gameManager;

    [Header("Referência ao Quiz (necessário para modo ByCorrectAnswers)")]
    public Quiz quiz;

    [Header("Painel")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeUsedText;

    [Header("Estrelas")]
    public Image star1;
    public Image star2;
    public Image star3;

    [Header("Botões")]
    public Button continueButton;
    public Button retryButton;

    [Header("Thresholds — Modo Tempo Restante")]
    public float threshold3Stars = 0.5f;
    public float threshold2Stars = 0.2f;

    [Header("Thresholds — Modo Termômetro")]
    public float thermo3Stars = 0.33f;
    public float thermo2Stars = 0.66f;

    [Header("Thresholds — Modo Acertos (porcentagem 0-1)")]
    [Tooltip("Ex: 0.75 = 75% de acertos para 3 estrelas")]
    public float correct3Stars = 0.75f;
    [Tooltip("Ex: 0.5 = 50% de acertos para 2 estrelas")]
    public float correct2Stars = 0.5f;

    [Header("Progresso")]
    [Tooltip("Chave do minigame. Deve ser igual ao PathBlocker. Ex: cacapalavras")]
    public string progressKey = "";

    [Header("Próxima cena")]
    public string nextSceneName = "";

    [Header("Diálogo pós-minigame")]
    public bool triggerNPCDialogueOnReturn = false;
    public string npcDialogueTriggerKey = "";

    public enum StarMode { ByRemainingTime, ByThermometer, ByCorrectAnswers }

    private bool _isGameOver = false;
    private bool _wasCompleted = false;
    private int _starsEarned = 0;

    private void OnEnable()
    {
        GameEvents.OnLevelComplete += OnLevelComplete;
        GameEvents.OnTimesUp += OnTimesUp;
        GameEvents.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelComplete -= OnLevelComplete;
        GameEvents.OnTimesUp -= OnTimesUp;
        GameEvents.OnGameOver -= OnGameOver;
    }

    private void Start()
    {
        panel.SetActive(false);
        continueButton.onClick.AddListener(OnContinue);
        retryButton.onClick.AddListener(OnRetry);
    }

    private void OnLevelComplete()
    {
        _wasCompleted = true;
        timer.StopTimer();
        StartCoroutine(ShowPanelDelayed(true));
    }

    private void OnTimesUp()
    {
        bool completed = starMode == StarMode.ByThermometer;
        _wasCompleted = completed;
        StartCoroutine(ShowPanelDelayed(completed));
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        _wasCompleted = false;
        timer.StopTimer();
        StartCoroutine(ShowPanelDelayed(false));
    }

    private IEnumerator ShowPanelDelayed(bool completed)
    {
        yield return new WaitForSeconds(0.8f);
        ShowPanel(completed);
    }

    private void ShowPanel(bool completed)
    {
        SoundManager.Instance?.StopMusic();
        SoundManager.Instance?.PlaySound2D(completed ? "level_complete" : "level_failed");
        panel.SetActive(true);

        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        if (completed)
        {
            _starsEarned = CalculateStars();
            titleText.text = _starsEarned == 3 ? "Incrível!" :
                             _starsEarned == 2 ? "Muito bem!" : "Continue tentando!";

            // Texto informativo conforme o modo
            if (starMode == StarMode.ByCorrectAnswers && quiz != null)
            {
                int correct = quiz.CorrectAnswers;
                int total = quiz.TotalQuestions;
                timeUsedText.text = $"Acertos: {correct}/{total}";
            }
            else if (starMode == StarMode.ByRemainingTime)
            {
                float timeUsed = timer.TotalTime - timer.RemainingTime;
                int min = Mathf.FloorToInt(timeUsed / 60);
                int sec = Mathf.FloorToInt(timeUsed % 60);
                timeUsedText.text = $"Tempo: {min:00}:{sec:00}";
            }
            else
            {
                float pct = gameManager != null
                    ? Mathf.Round(gameManager.ThermometerPercent * 100f) : 0f;
                timeUsedText.text = $"Temperatura: {pct:0}%";
            }

            SetStars(_starsEarned);
        }
        else
        {
            _starsEarned = 0;
            titleText.text = _isGameOver ? "Que calor! Tente novamente!" : "Tempo esgotado!";
            timeUsedText.text = "";
            SetStars(0);
        }

        StartCoroutine(AnimateStars());
    }

    private int CalculateStars()
    {
        if (starMode == StarMode.ByCorrectAnswers)
        {
            if (quiz == null || quiz.TotalQuestions == 0) return 1;

            float pct = (float)quiz.CorrectAnswers / quiz.TotalQuestions;

            if (pct >= correct3Stars) return 3;
            if (pct >= correct2Stars) return 2;
            return 1;
        }
        else if (starMode == StarMode.ByRemainingTime)
        {
            float pct = timer.RemainingTime / timer.TotalTime;
            if (pct >= threshold3Stars) return 3;
            if (pct >= threshold2Stars) return 2;
            return 1;
        }
        else // ByThermometer
        {
            if (gameManager == null) return 1;
            float pct = gameManager.ThermometerPercent;
            if (pct <= thermo3Stars) return 3;
            if (pct <= thermo2Stars) return 2;
            return 1;
        }
    }

    private void SetStars(int count)
    {
        SetStar(star1, count >= 1);
        SetStar(star2, count >= 2);
        SetStar(star3, count >= 3);
        star1.transform.localScale = Vector3.zero;
        star2.transform.localScale = Vector3.zero;
        star3.transform.localScale = Vector3.zero;
    }

    private void SetStar(Image star, bool filled)
    {
        star.color = filled ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
    }

    private IEnumerator AnimateStars()
    {
        yield return new WaitForSeconds(0.3f);
        yield return AnimateStar(star1);
        yield return new WaitForSeconds(0.15f);
        yield return AnimateStar(star2);
        yield return new WaitForSeconds(0.15f);
        yield return AnimateStar(star3);
    }

    private IEnumerator AnimateStar(Image star)
    {
        float t = 0f;
        float duration = 0.3f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t / duration));
            star.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        star.transform.localScale = Vector3.one;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private void OnContinue()
    {
        Time.timeScale = 1f;

        if (_wasCompleted && !string.IsNullOrEmpty(progressKey) &&
            ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SetCompleted(progressKey);
        }

        if (_wasCompleted && _starsEarned > 0 && !string.IsNullOrEmpty(progressKey) &&
            SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetStars(progressKey, _starsEarned);
        }

        if (_wasCompleted && triggerNPCDialogueOnReturn && !string.IsNullOrEmpty(npcDialogueTriggerKey))
        {
            PlayerPrefs.SetInt(npcDialogueTriggerKey, 1);
            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey("returning_from_minigame"))
        {
            PlayerPrefs.SetInt("returning_from_minigame", 1);
            PlayerPrefs.Save();
        }

        string targetScene = !string.IsNullOrEmpty(nextSceneName) ? nextSceneName : "MainScene";
        SceneManager.LoadScene(targetScene);
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        PlayerPrefs.DeleteKey("returning_from_minigame");
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}