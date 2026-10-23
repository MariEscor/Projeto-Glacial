using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;

    [Header("Progresso")]
    [Tooltip("Se preenchido, marca esta chave como completada ao fim do diálogo")]
    public string progressKeyOnDialogueEnd = "";

    [Tooltip("Se preenchido, carrega esta cena ao fim do diálogo")]
    public string sceneToLoadOnDialogueEnd = "";

    [Header("Cooldown após diálogo")]
    [Tooltip("Segundos de espera antes de poder interagir de novo")]
    public float interactionCooldown = 1.5f;

    [Header("Diálogo pós-minigame")]
    [Tooltip("Deve ser igual ao npcDialogueTriggerKey do LevelCompletePanel da cena reciclagem")]
    public string autoStartTriggerKey = "";

    [Tooltip("Diálogo alternativo usado quando iniciado automaticamente após o minigame")]
    public NPCDialogue postMinigameDialogue;

    [Tooltip("Cena a carregar ao fim do diálogo pós-minigame (ex: CompleteGame)")]
    public string postMinigameSceneToLoad = "";

    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool _onCooldown = false;
    private bool _textJustCompleted = false;
    private bool _isPostMinigameDialogue = false;
    private bool _choicesActive = false;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
        CheckAutoStart();
    }

    /// <summary>Verifica se deve iniciar diálogo automaticamente ao voltar do minigame.</summary>
    private void CheckAutoStart()
    {
        if (string.IsNullOrEmpty(autoStartTriggerKey)) return;
        if (!PlayerPrefs.HasKey(autoStartTriggerKey)) return;

        // Limpa a flag para não repetir
        PlayerPrefs.DeleteKey(autoStartTriggerKey);
        PlayerPrefs.Save();

        _isPostMinigameDialogue = true;

        // Pequeno delay para garantir que a cena carregou completamente
        StartCoroutine(AutoStartDialogueDelayed());
    }

    private IEnumerator AutoStartDialogueDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        StartDialogue();
    }

    public bool CanInteract() => !_onCooldown;

    public void Interact()
    {
        if (dialogueData == null) return;

        if (isDialogueActive)
            NextLine();
        else
        {
            _isPostMinigameDialogue = false;
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        _textJustCompleted = false;

        // Usa o diálogo pós-minigame se disponível e for esse o contexto
        NPCDialogue data = (_isPostMinigameDialogue && postMinigameDialogue != null)
            ? postMinigameDialogue
            : dialogueData;

        dialogueUI.SetNPCInfo(data.npcName, data.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetActiveNPC(this);

        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    // Retorna o diálogo ativo no momento
    NPCDialogue ActiveDialogue => (_isPostMinigameDialogue && postMinigameDialogue != null)
        ? postMinigameDialogue
        : dialogueData;

    void NextLine()
    {
        if (_choicesActive) return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(ActiveDialogue.dialogueLines[dialogueIndex]);
            isTyping = false;
            _textJustCompleted = true;
            StartCoroutine(WaitForNextInput());
            return;
        }

        if (_textJustCompleted)
            return;

        AdvanceLine();
    }

    IEnumerator WaitForNextInput()
    {
        yield return new WaitForSeconds(0.15f);
        _textJustCompleted = false;
    }

    void AdvanceLine()
    {
        _choicesActive = false;
        dialogueUI.ClearChoices();
        _textJustCompleted = false;

        NPCDialogue data = ActiveDialogue;

        // Verifica fim de diálogo
        if (data.endDialogueLines.Length > dialogueIndex &&
            data.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Verifica escolhas
        foreach (DialogueChoice choice in data.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(choice);
                return;
            }
        }

        // Verifica pulos forçados
        if (data.jumps != null)
        {
            foreach (DialogueJump jump in data.jumps)
            {
                if (jump.fromIndex == dialogueIndex)
                {
                    dialogueIndex = jump.toIndex;
                    DisplayCurrentLine();
                    return;
                }
            }
        }

        // Avança normalmente
        if (++dialogueIndex < data.dialogueLines.Length)
            DisplayCurrentLine();
        else
            EndDialogue();
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        _textJustCompleted = false;
        dialogueUI.SetDialogueText("");

        NPCDialogue data = ActiveDialogue;

        foreach (char letter in data.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            yield return new WaitForSeconds(data.typingSpeed);
        }

        isTyping = false;

        if (data.autoProgressLines.Length > dialogueIndex &&
            data.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(data.autoProgressDelay);
            AdvanceLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        _choicesActive = true;
        for (int i = 0; i < choice.choices.Length; i++)
        {
            // nextDialogueIndexes pode estar vazio se a escolha só carrega cena
            int nextIndex = (choice.nextDialogueIndexes != null && i < choice.nextDialogueIndexes.Length)
                ? choice.nextDialogueIndexes[i] : -1;

            string sceneName = (choice.sceneToLoad != null && i < choice.sceneToLoad.Length)
                ? choice.sceneToLoad[i] : "";

            dialogueUI.CreateChoiceButton(choice.choices[i],
                () => ChooseOption(nextIndex, sceneName));
        }
    }

    void ChooseOption(int nextIndex, string sceneName)
    {
        _choicesActive = false;
        SoundManager.Instance?.PlaySound2D("dialogue_next");
        dialogueUI.ClearChoices();

        if (!string.IsNullOrEmpty(sceneName))
        {
            dialogueUI.ShowDialogueUI(false);

            // Se for minigame (não é cena final), salva posição do player
            bool isFinalScene = sceneName == postMinigameSceneToLoad ||
                                (!string.IsNullOrEmpty(postMinigameSceneToLoad) &&
                                 sceneName == postMinigameSceneToLoad);

            if (!isFinalScene)
            {
                LoadMinigame(sceneName);
            }
            else
            {
                // Cena final (ex: CompleteGame) — vai direto sem salvar posição
                PauseController.SetPause(false);
                SceneManager.LoadScene(sceneName);
            }
            return;
        }

        if (nextIndex < 0)
        {
            EndDialogue();
            return;
        }

        dialogueIndex = nextIndex;
        DisplayCurrentLine();
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        _textJustCompleted = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);

        PauseController.SetPause(false);

        // Se era o diálogo pós-minigame, carrega a cena final
        if (_isPostMinigameDialogue && !string.IsNullOrEmpty(postMinigameSceneToLoad))
        {
            _isPostMinigameDialogue = false;
            SceneManager.LoadScene(postMinigameSceneToLoad);
            return;
        }

        _isPostMinigameDialogue = false;

        if (!string.IsNullOrEmpty(progressKeyOnDialogueEnd) && ProgressManager.Instance != null)
            ProgressManager.Instance.SetCompleted(progressKeyOnDialogueEnd);

        if (!string.IsNullOrEmpty(sceneToLoadOnDialogueEnd))
        {
            // Verifica se é cena final ou minigame
            if (sceneToLoadOnDialogueEnd == postMinigameSceneToLoad)
            {
                PauseController.SetPause(false);
                SceneManager.LoadScene(sceneToLoadOnDialogueEnd);
            }
            else
            {
                LoadMinigame(sceneToLoadOnDialogueEnd);
            }
            return;
        }

        StartCoroutine(InteractionCooldown());
    }

    IEnumerator InteractionCooldown()
    {
        _onCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        _onCooldown = false;
    }

    void DisplayCurrentLine()
    {
        SoundManager.Instance?.PlaySound2D("dialogue_next");
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    void LoadMinigame(string sceneName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;

            // Salva no PlayerPrefs (lido pelo SaveController)
            PlayerPrefs.SetFloat("player_return_x", pos.x);
            PlayerPrefs.SetFloat("player_return_y", pos.y);
            PlayerPrefs.SetFloat("player_return_z", pos.z);
            PlayerPrefs.SetString("last_minigame_scene", sceneName);
            PlayerPrefs.SetInt("returning_from_minigame", 1);
            PlayerPrefs.Save();

            // Salva no SaveSystem (persistente na build)
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveReturnPointPosition(pos, sceneName);
                Debug.Log($"[NPC] Posição salva no SaveSystem: {pos} | Cena: {sceneName}");
            }
            else
            {
                Debug.LogWarning("[NPC] SaveSystem.Instance é null ao tentar salvar posição!");
            }
        }

        PauseController.SetPause(false);
        SceneManager.LoadScene(sceneName);
    }
}