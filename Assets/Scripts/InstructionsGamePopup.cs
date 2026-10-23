using System.Collections;
using UnityEngine;

public class InstructionsGamePopup : MonoBehaviour
{
    [Header("Painel do popup")]
    public GameObject popupPanel;

    [Header("Parar o jogador enquanto popup está aberto")]
    [Tooltip("Arraste o GameObject do jogador aqui para travar o movimento")]
    public GameObject playerObject;

    [Header("Mostrar sempre ou só na primeira vez?")]
    [Tooltip("Se true, só aparece uma vez. Se false, aparece toda vez que entrar na cena.")]
    public bool showOnlyOnce = true;

    private const string SHOWN_KEY = "instructions_shown";

    private CanvasGroup _canvasGroup;
    private RectTransform _panelRect;

    private void Awake()
    {
        _canvasGroup = popupPanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = popupPanel.AddComponent<CanvasGroup>();

        _panelRect = popupPanel.GetComponent<RectTransform>();
        popupPanel.SetActive(false);
    }

    private void Start()
    {
        // Não mostra se já foi exibido antes
        bool alreadyShown = showOnlyOnce && PlayerPrefs.GetInt(SHOWN_KEY, 0) == 1;

        // Não mostra se estamos voltando de um minigame
        bool returningFromMinigame = PlayerPrefs.HasKey("returning_from_minigame");

        if (!alreadyShown && !returningFromMinigame)
            StartCoroutine(ShowPopup());
    }

    private IEnumerator ShowPopup()
    {
        SetPlayerMovement(false);

        popupPanel.SetActive(true);
        _canvasGroup.alpha = 0f;
        _panelRect.localScale = Vector3.one * 0.85f;

        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _canvasGroup.alpha = t;
            _panelRect.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _panelRect.localScale = Vector3.one;
    }

    public void OnContinueClicked()
    {
        StartCoroutine(HidePopup());

        if (showOnlyOnce)
        {
            PlayerPrefs.SetInt(SHOWN_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator HidePopup()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _canvasGroup.alpha = 1f - t;
            _panelRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, t);
            yield return null;
        }

        popupPanel.SetActive(false);
        SetPlayerMovement(true);
    }

    private void SetPlayerMovement(bool enabled)
    {
        if (playerObject == null) return;
        var movement = playerObject.GetComponent<Movement>();
        if (movement != null)
            movement.enabled = enabled;
    }
}