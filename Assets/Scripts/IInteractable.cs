using UnityEngine;

public interface IInteractable
{
    // Em interfaces, os métodos são públicos por padrão
    void Interact();

    bool CanInteract();
}