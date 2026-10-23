using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Termômetro")]
    [SerializeField] Transform thermometerFill;
    [SerializeField] float minTemp = -5f;
    [SerializeField] float maxTemp = 5f;
    [SerializeField] float goodValue = 1f;
    [SerializeField] float badValue = 1f;

    float currentTemp = 0f;
    bool _gameOver = false;

    public float ThermometerPercent
    {
        get { return Mathf.InverseLerp(minTemp, maxTemp, currentTemp); }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentTemp = minTemp;
        UpdateThermometer();
    }

    void UpdateThermometer()
    {
        float normalized = Mathf.InverseLerp(minTemp, maxTemp, currentTemp);
        thermometerFill.localScale = new Vector3(1f, normalized, 1f);
    }

    public void GoodItem()
    {
        if (_gameOver) return;
        currentTemp = Mathf.Clamp(currentTemp - goodValue, minTemp, maxTemp);
        UpdateThermometer();
    }

    public void BadItem()
    {
        if (_gameOver) return;
        currentTemp = Mathf.Clamp(currentTemp + badValue, minTemp, maxTemp);
        UpdateThermometer();

        if (currentTemp >= maxTemp)
        {
            _gameOver = true;
            GameEvents.GameOverMethod();
        }
    }
}