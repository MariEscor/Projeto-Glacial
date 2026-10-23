using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonController : MonoBehaviour
{
    [Header("Popup Panel")]
    [SerializeField] private GameObject popupCanvas;

    [Header("Transition")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Scene")]
    [SerializeField] private string targetScene = "MainScene";

    private CanvasGroup popupCanvasGroup;
    private CanvasGroup transitionCanvasGroup;
    private RectTransform popupRect;

    private void Awake()
    {
        popupCanvas.SetActive(false);

        popupCanvasGroup = popupCanvas.GetComponent<CanvasGroup>();
        if (popupCanvasGroup == null)
            popupCanvasGroup = popupCanvas.AddComponent<CanvasGroup>();

        popupRect = popupCanvas.GetComponent<RectTransform>();

        if (transitionPanel != null)
        {
            transitionCanvasGroup = transitionPanel.GetComponent<CanvasGroup>();
            if (transitionCanvasGroup == null)
                transitionCanvasGroup = transitionPanel.AddComponent<CanvasGroup>();

            transitionCanvasGroup.alpha = 0f;
        }
    }

    public void OnBackButtonClicked()
    {
        popupCanvas.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimatePopupIn());

        // Pausa o timer
        Timer timer = FindAnyObjectByType<Timer>();
        if (timer != null) timer.StopTimer();
    }

    public void OnNoClicked()
    {
        StopAllCoroutines();
        StartCoroutine(AnimatePopupOut());

        // Retoma o timer
        Timer timer = FindAnyObjectByType<Timer>();
        if (timer != null) timer.StartTimer();
    }

    public void OnYesClicked()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionToMainScene());
    }

    // 🔥 ANIMAÇÃO DE ENTRADA (mais suave e profissional)
    private IEnumerator AnimatePopupIn()
    {
        float duration = 0.25f;
        float elapsed = 0f;

        popupCanvasGroup.alpha = 0f;
        popupRect.localScale = Vector3.one * 0.85f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            popupCanvasGroup.alpha = t;
            popupRect.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);

            yield return null;
        }

        popupCanvasGroup.alpha = 1f;
        popupRect.localScale = Vector3.one;
    }

    // 🔥 ANIMAÇÃO DE SAÍDA
    private IEnumerator AnimatePopupOut()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            popupCanvasGroup.alpha = 1f - t;
            popupRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, t);

            yield return null;
        }

        popupCanvas.SetActive(false);
    }

    private IEnumerator TransitionToMainScene()
    {
        yield return StartCoroutine(AnimatePopupOut());

        if (transitionCanvasGroup != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

                transitionCanvasGroup.alpha = t;
                yield return null;
            }

            transitionCanvasGroup.alpha = 1f;
        }

        SceneManager.LoadScene(targetScene);
    }
}