using UnityEngine;

/// <summary>
/// Coloque este script no GameController (que já existe na cena).
/// Arraste o GameObject "Menu" no campo Menu Object pelo Inspector.
/// ESC abre/fecha o menu e pausa/despausa o jogador automaticamente.
/// </summary>
public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; } = false;

    [Tooltip("Arraste aqui o GameObject 'Menu' que está dentro de UI")]
    public GameObject menuObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();
    }

    /// <summary>Pode ser chamado por botão também (ex: botão de fechar o menu).</summary>
    public void ToggleMenu()
    {
        if (menuObject == null)
        {
            Debug.LogWarning("[PauseController] Menu Object não atribuído no Inspector!");
            return;
        }

        bool abrindo = !menuObject.activeSelf;
        menuObject.SetActive(abrindo);
        SetPause(abrindo);
    }

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
    }

    // Reseta automaticamente ao carregar qualquer cena nova,
    // evitando que o flag fique preso entre cenas.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetOnSceneLoad()
    {
        IsGamePaused = false;
    }
}