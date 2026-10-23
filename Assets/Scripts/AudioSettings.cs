using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloque este script no GameObject do menu de configurações.
/// Conecte os sliders de Música e Efeitos Sonoros no Inspector.
/// </summary>
public class AudioSettings : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Inicializa os sliders com os valores salvos
        if (SoundManager.Instance != null)
        {
            musicSlider.value = SoundManager.Instance.MusicVolume;
            sfxSlider.value   = SoundManager.Instance.SFXVolume;
        }

        // Conecta os eventos
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    private void OnMusicChanged(float value)
    {
        SoundManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value);
    }
}
