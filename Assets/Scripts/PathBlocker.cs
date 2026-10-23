using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PathBlocker : MonoBehaviour
{
    [Header("Condição para liberar")]
    public string progressKey = "";

    [Header("Visual")]
    public SpriteRenderer blockerSprite;

    [Header("Animação")]
    [Tooltip("Duração do fade ao desaparecer")]
    public float fadeDuration = 0.5f;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Se o minigame já foi completado (vindo de um save), some imediatamente sem animação
        if (ProgressManager.Instance != null && ProgressManager.Instance.IsCompleted(progressKey))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPathUnlocked += OnPathUnlocked;
    }

    private void OnDisable()
    {
        GameEvents.OnPathUnlocked -= OnPathUnlocked;
    }

    private void OnPathUnlocked(string key)
    {
        if (key == progressKey)
            StartCoroutine(FadeAndDisable());
    }

    private IEnumerator FadeAndDisable()
    {
        // desativa collider imediatamente para o jogador passar
        _collider.enabled = false;

        // fade do sprite
        if (blockerSprite != null)
        {
            float elapsed = 0f;
            Color startColor = blockerSprite.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                blockerSprite.color = new Color(
                    startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }
}