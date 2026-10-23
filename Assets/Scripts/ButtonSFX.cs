using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adicione este script em qualquer botão para tocar som ao clicar.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private string soundID = "button_click";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance?.PlaySound2D(soundID);
        });
    }
}
