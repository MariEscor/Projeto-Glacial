using UnityEngine;
using Unity.Cinemachine;

public class ReturnPoint : MonoBehaviour
{
    [Tooltip("Nome da cena do minigame associado. Ex: cacapalavras")]
    public string progressKey = "";

    private void Start()
    {
        if (!PlayerPrefs.HasKey("returning_from_minigame")) return;

        string lastScene = PlayerPrefs.GetString("last_minigame_scene", "");
        if (string.IsNullOrEmpty(progressKey) || string.IsNullOrEmpty(lastScene)) return;

        string sceneLower = lastScene.ToLower().Trim();
        string keyLower = progressKey.ToLower().Trim();
        bool matches = sceneLower.Contains(keyLower) || keyLower.Contains(sceneLower);
        if (!matches) return;

        if (SaveSystem.Instance == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 pos = SaveSystem.Instance.GetReturnPointPosition();
        player.transform.position = pos;
        Debug.Log($"[ReturnPoint] Reposicionando '{gameObject.name}' para {pos}");

        // Força a câmera do Cinemachine a pular imediatamente para o jogador
        var vcam = Object.FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
            vcam.OnTargetObjectWarped(player.transform, pos - player.transform.position);

        // Limpa flags
        PlayerPrefs.DeleteKey("returning_from_minigame");
        PlayerPrefs.DeleteKey("last_minigame_scene");
        PlayerPrefs.Save();
        SaveSystem.Instance.ClearReturnPoint();
    }
}