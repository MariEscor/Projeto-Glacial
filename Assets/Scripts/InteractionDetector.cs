using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    private Collider2D colliderInRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    void Update()
    {
        // Atualiza o ícone a cada frame com base no CanInteract() atual
        // Isso garante que o ícone apareça depois do cooldown terminar
        if (colliderInRange != null)
        {
            bool canInteract = interactableInRange != null && interactableInRange.CanInteract();
            interactionIcon.SetActive(canInteract);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && interactableInRange != null && interactableInRange.CanInteract())
        {
            interactableInRange.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            interactableInRange = interactable;
            colliderInRange = collision;
            interactionIcon.SetActive(interactable.CanInteract());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == colliderInRange)
        {
            interactableInRange = null;
            colliderInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}