using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainMenuPanel;
    public GameObject saveMenuPanel;

    [Header("Área do Professor")]
    [Tooltip("Arraste aqui o GameObject AreaDoProfessor da hierarquia")]
    public GameObject areaDoProfessorPanel;

    [Header("Botão Continuar")]
    public Button continueButton;

    [Header("Cena do jogo")]
    public string gameSceneName = "Game";

    private void Start()
    {
        // Estado inicial: só o menu principal visível
        mainMenuPanel.SetActive(true);
        saveMenuPanel.SetActive(false);

        if (areaDoProfessorPanel != null)
            areaDoProfessorPanel.SetActive(false);

        if (continueButton != null)
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSave();
            continueButton.interactable = hasSave;
        }
    }

    // ── Menu principal ───────────────────────────────────────────────

    public void OnClickPlay()
    {
        if (continueButton != null)
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSave();
            continueButton.interactable = hasSave;
        }

        mainMenuPanel.SetActive(false);
        saveMenuPanel.SetActive(true);
    }

    /// <summary>Abre a Área do Professor escondendo o menu principal.</summary>
    public void OnClickAreaDoProfessor()
    {
        mainMenuPanel.SetActive(false);
        if (areaDoProfessorPanel != null)
            areaDoProfessorPanel.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // ── Submenu de save ──────────────────────────────────────────────

    public void OnClickNewGame()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.ResetAll();
        else if (SaveSystem.Instance != null)
            SaveSystem.Instance.DeleteSave();

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickContinue()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickBack()
    {
        saveMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // ── Voltar da Área do Professor ──────────────────────────────────

    /// <summary>
    /// Chame este método no botão "Voltar" dentro da AreaDoProfessor.
    /// Ele desativa a área e reativa o menu principal.
    /// </summary>
    public void OnClickVoltarAreaDoProfessor()
    {
        if (areaDoProfessorPanel != null)
            areaDoProfessorPanel.SetActive(false);

        mainMenuPanel.SetActive(true);
    }
}