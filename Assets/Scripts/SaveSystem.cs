using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [System.Serializable]
    private class GameSaveData
    {
        public List<string> completedKeys = new List<string>();
        public List<string> starKeys = new List<string>(); // chaves
        public List<int> starValues = new List<int>();    // estrelas (1-3)
        public string lastCompletedKey = "";
        public string lastMinigameScene = "";
        public float playerX;
        public float playerY;
        public float playerZ;
        public float returnPointX;
        public float returnPointY;
        public bool hasSave;
        public bool hasReturnPoint;
    }

    private GameSaveData _data = new GameSaveData();
    private string _savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "missao_atmosfera.json");
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && _data.hasSave)
            SavePlayerPosition(player.transform.position);
    }

    // ── Progresso ───────────────────────────────────

    public bool HasSave() => _data.hasSave;

    public void SetCompleted(string key, Vector3 returnPosition)
    {
        if (!_data.completedKeys.Contains(key))
            _data.completedKeys.Add(key);

        _data.lastCompletedKey = key;
        _data.playerX = returnPosition.x;
        _data.playerY = returnPosition.y;
        _data.playerZ = returnPosition.z;
        _data.hasSave = true;
        Save();
    }

    public bool IsCompleted(string key) => _data.completedKeys.Contains(key);
    public string GetLastCompletedKey() => _data.lastCompletedKey;

    // ── Estrelas ─────────────────────────────────────

    /// <summary>Salva a quantidade de estrelas de um minigame (só atualiza se for maior).</summary>
    public void SetStars(string key, int stars)
    {
        int idx = _data.starKeys.IndexOf(key);
        if (idx >= 0)
        {
            // Só sobrescreve se conseguiu mais estrelas
            if (stars > _data.starValues[idx])
                _data.starValues[idx] = stars;
        }
        else
        {
            _data.starKeys.Add(key);
            _data.starValues.Add(stars);
        }
        Save();
    }

    /// <summary>Retorna estrelas salvas (0 se nunca jogou).</summary>
    public int GetStars(string key)
    {
        int idx = _data.starKeys.IndexOf(key);
        return idx >= 0 ? _data.starValues[idx] : 0;
    }

    // ── ReturnPoint ──────────────────────────────────

    public void SaveReturnPointPosition(Vector3 position, string minigameScene)
    {
        _data.returnPointX = position.x;
        _data.returnPointY = position.y;
        _data.lastMinigameScene = minigameScene;
        _data.hasReturnPoint = true;
        _data.hasSave = true;
        Save();
    }

    public bool HasReturnPoint() => _data.hasReturnPoint;
    public bool HasReturnPointFor(string minigameScene) =>
        _data.hasReturnPoint && _data.lastMinigameScene == minigameScene;
    public string GetLastMinigameScene() => _data.lastMinigameScene;
    public Vector3 GetReturnPointPosition() =>
        new Vector3(_data.returnPointX, _data.returnPointY, 0f);

    public void ClearReturnPoint()
    {
        _data.hasReturnPoint = false;
        _data.lastMinigameScene = "";
        Save();
    }

    // ── Posição do jogador ───────────────────────────

    public Vector3 GetPlayerPosition() =>
        new Vector3(_data.playerX, _data.playerY, _data.playerZ);

    public void SavePlayerPosition(Vector3 position)
    {
        if (!_data.hasSave) return;
        _data.playerX = position.x;
        _data.playerY = position.y;
        _data.playerZ = position.z;
        Save();
    }

    // ── Novo Jogo ────────────────────────────────────

    public void DeleteSave()
    {
        _data = new GameSaveData();
        if (File.Exists(_savePath))
            File.Delete(_savePath);
    }

    // ── Interno ──────────────────────────────────────

    private void Save()
    {
        File.WriteAllText(_savePath, JsonUtility.ToJson(_data, true));
    }

    private void Load()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _data = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
        }
        else
        {
            _data = new GameSaveData();
        }
    }
}