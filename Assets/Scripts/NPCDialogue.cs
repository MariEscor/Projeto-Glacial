using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    public DialogueChoice[] choices;

    /// <summary>
    /// Pulos forçados: quando chegar no índice "fromIndex",
    /// após exibir a linha, pula para "toIndex" em vez de avançar +1.
    /// Use para ramos que precisam convergir (ex: linha 13 → pula para 21).
    /// </summary>
    public DialogueJump[] jumps;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public string[] sceneToLoad;
}

[System.Serializable]
public class DialogueJump
{
    [Tooltip("Índice da linha que deve causar o pulo")]
    public int fromIndex;
    [Tooltip("Índice para onde o diálogo deve ir após essa linha")]
    public int toIndex;
}