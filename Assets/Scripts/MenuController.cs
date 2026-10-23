using UnityEngine;

/// <summary>
/// Mantém o menu fechado ao iniciar a cena.
/// A abertura/fechamento agora é feita exclusivamente pelo PauseController (tecla ESC).
/// </summary>
public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
        menuCanvas.SetActive(false);
    }
}