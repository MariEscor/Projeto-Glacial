using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private SoundLibrary sfxLibrary;
    [SerializeField] private AudioSource sfx2DSource;

    [Header("Música de Fundo")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("Efeitos Sonoros")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Carrega volumes salvos
        musicVolume = PlayerPrefs.GetFloat("vol_music", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("vol_sfx", sfxVolume);

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfx2DSource != null)
            sfx2DSource.volume = sfxVolume;
    }

    // ── SFX ─────────────────────────────────────────────────────────

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos);
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip != null)
            sfx2DSource.PlayOneShot(clip, sfxVolume);
    }

    // ── Volume ───────────────────────────────────────────────────────

    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("vol_music", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfx2DSource != null)
            sfx2DSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("vol_sfx", sfxVolume);
        PlayerPrefs.Save();
    }

    // ── Música ───────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(clip));
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeOut(musicSource, crossfadeDuration));
    }

    // ── Coroutines ───────────────────────────────────────────────────

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossfadeDuration);
                yield return null;
            }
        }

        musicSource.clip = newClip;
        musicSource.Play();

        float elapsed2 = 0f;
        while (elapsed2 < crossfadeDuration)
        {
            elapsed2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed2 / crossfadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.Stop();
        source.volume = musicVolume;
    }
}