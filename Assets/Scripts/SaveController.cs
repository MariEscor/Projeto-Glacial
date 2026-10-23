using Unity.Cinemachine;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    void Awake()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("O arquivo de save está em: " + saveLocation);
    }

    void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        var confiner = Object.FindAnyObjectByType<CinemachineConfiner2D>();

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = confiner != null ? confiner.BoundingShape2D.gameObject.name : ""
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        bool returningFromMinigame = PlayerPrefs.HasKey("returning_from_minigame");

        if (returningFromMinigame)
        {
            Vector3 returnPos = Vector3.zero;
            bool found = false;

            // Fonte primária: SaveSystem (confiável na build)
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasReturnPoint())
            {
                returnPos = SaveSystem.Instance.GetReturnPointPosition();
                found = true;
                Debug.Log($"[SaveController] Voltando via SaveSystem: {returnPos}");
            }

            // Fallback: PlayerPrefs
            if (!found)
            {
                returnPos = new Vector3(
                    PlayerPrefs.GetFloat("player_return_x", 0f),
                    PlayerPrefs.GetFloat("player_return_y", 0f),
                    PlayerPrefs.GetFloat("player_return_z", 0f)
                );
                found = true;
                Debug.Log($"[SaveController] Voltando via PlayerPrefs: {returnPos}");
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && found)
            {
                player.transform.position = returnPos;
                Debug.Log($"[SaveController] Player reposicionado para {returnPos}");

                var vcam = Object.FindAnyObjectByType<CinemachineCamera>();
                if (vcam != null)
                    vcam.OnTargetObjectWarped(player.transform, Vector3.zero);
            }

            RestoreConfiner();

            // Limpa flags
            PlayerPrefs.DeleteKey("returning_from_minigame");
            PlayerPrefs.DeleteKey("player_return_x");
            PlayerPrefs.DeleteKey("player_return_y");
            PlayerPrefs.DeleteKey("player_return_z");
            PlayerPrefs.DeleteKey("last_minigame_scene");
            PlayerPrefs.Save();

            if (SaveSystem.Instance != null)
                SaveSystem.Instance.ClearReturnPoint();
        }
        else if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            RestoreConfiner(saveData.mapBoundary);
        }
        else
        {
            SaveGame();
        }
    }

    private void RestoreConfiner(string boundaryName = "")
    {
        var confiner = Object.FindAnyObjectByType<CinemachineConfiner2D>();
        if (confiner == null) return;

        string name = boundaryName;
        if (string.IsNullOrEmpty(name) && File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            name = saveData.mapBoundary;
        }

        if (!string.IsNullOrEmpty(name))
        {
            GameObject boundaryObj = GameObject.Find(name);
            if (boundaryObj != null)
                confiner.BoundingShape2D = boundaryObj.GetComponent<PolygonCollider2D>();
        }
    }
}