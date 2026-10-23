using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionPopup : MonoBehaviour
{
    [Header("Referências")]
    public Timer timer;

    [Header("UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Button okButton;

    [Header("Conteúdo")]
    [TextArea(3, 8)]
    public string title = "Como jogar";
    [TextArea(5, 12)]
    public string instructions = "Escreva aqui as instruções da fase.";

    [Header("Comportamento")]
    public bool pauseWhileOpen = true;

    [Header("Objetos para desativar durante o popup")]
    [Tooltip("Arraste aqui tudo que não deve receber cliques enquanto o popup estiver aberto")]
    public GameObject[] objectsToDisableDuringPopup;

    private void Start()
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = instructions;

        // desativa objetos do jogo para evitar cliques acidentais
        foreach (var obj in objectsToDisableDuringPopup)
            if (obj != null) obj.SetActive(false);

        popupPanel.SetActive(true);

        if (pauseWhileOpen)
            Time.timeScale = 0f;

        okButton.onClick.AddListener(OnOkClicked);
    }

    private void OnOkClicked()
    {
        Time.timeScale = 1f;
        popupPanel.SetActive(false);

        // reativa os objetos do jogo
        foreach (var obj in objectsToDisableDuringPopup)
            if (obj != null) obj.SetActive(true);

        if (timer != null)
            timer.StartTimer();
    }
}