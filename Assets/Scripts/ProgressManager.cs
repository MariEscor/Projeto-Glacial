using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    public const string KEY_DIALOGUE_NPC0 = "dialogue_npc0";
    public const string KEY_CACAPALAVRAS = "minigame_cacapalavras";
    public const string KEY_COLETA = "minigame_coleta";
    public const string KEY_RECICLAGEM = "minigame_reciclagem";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsCompleted(string key)
    {
        if (SaveSystem.Instance != null)
            return SaveSystem.Instance.IsCompleted(key);

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void SetCompleted(string key)
    {
        Vector3 returnPos = Vector3.zero;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            returnPos = player.transform.position;

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SetCompleted(key, returnPos);
        else
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }

        GameEvents.PathUnlockedMethod(key);
    }

    public void ResetAll()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.DeleteSave();

        // ✅ Preserva instructions_shown ao resetar — não deve sumir com Novo Jogo
        int instructionsShown = PlayerPrefs.GetInt("instructions_shown", 0);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("instructions_shown", instructionsShown);
        PlayerPrefs.Save();
    }

    public void SaveReturnPosition(Vector3 position)
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave()) return;

        PlayerPrefs.SetFloat("return_x", position.x);
        PlayerPrefs.SetFloat("return_y", position.y);
        PlayerPrefs.SetFloat("return_z", position.z);
        PlayerPrefs.Save();
    }

    public Vector3 GetReturnPosition()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
            return SaveSystem.Instance.GetPlayerPosition();

        if (!HasReturnPosition()) return Vector3.zero;
        return new Vector3(
            PlayerPrefs.GetFloat("return_x"),
            PlayerPrefs.GetFloat("return_y"),
            PlayerPrefs.GetFloat("return_z")
        );
    }

    public bool HasReturnPosition()
    {
        if (SaveSystem.Instance != null)
            return SaveSystem.Instance.HasSave();

        return PlayerPrefs.HasKey("return_x");
    }
}