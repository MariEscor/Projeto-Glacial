using UnityEngine;

/// <summary>
/// Coloque este script em um GameObject de cada cena.
/// Ele avisa o SoundManager qual música deve tocar.
/// </summary>
public class MusicController : MonoBehaviour
{
    [Tooltip("Música de fundo desta cena")]
    [SerializeField] private AudioClip sceneMusic;

    private void Start()
    {
        if (SoundManager.Instance != null && sceneMusic != null)
            SoundManager.Instance.PlayMusic(sceneMusic);
    }
}
