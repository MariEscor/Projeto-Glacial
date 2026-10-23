using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remainingTime = 60f;

    public float TotalTime => _totalTime;
    public float RemainingTime => remainingTime;
    public bool IsRunning => _running;

    private float _totalTime;
    private bool _running = false; // começa parado — aguarda StartTimer()
    private bool _timesUpFired = false;

    void Start()
    {
        _totalTime = remainingTime;
        UpdateDisplay(); // mostra o valor inicial parado
    }

    public void StartTimer()
    {
        _running = true;
    }

    public void StopTimer()
    {
        _running = false;
    }

    private void Update()
    {
        if (!_running) return;

        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) remainingTime = 0f;

            float pct = remainingTime / _totalTime;
            //timerText.color = pct < 0.2f ? Color.red : Color.white;
        }
        else
        {
            _running = false;
            timerText.color = Color.red;

            if (!_timesUpFired)
            {
                _timesUpFired = true;
                GameEvents.TimesUpMethod();
            }
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}