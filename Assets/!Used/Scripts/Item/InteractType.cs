using UnityEngine;

public class InteractType : MonoBehaviour
{
    public InteractionType interactionType;
    public void Hover(Inventory inventory)
    {
        switch (interactionType)
        {
            case InteractionType.Storage:
                break;
        }
    }

    public void Interact(Inventory inventory)
    {
        switch (interactionType)
        {
            case InteractionType.Storage:
                Debug.Log($"<color=black>[Interact Type]</color> Interacted with Storage");
                Storage storage = GetComponent<Storage>();
                if (storage != null) storage.Interact();
                else Debug.Log("<color=black>[Interact Type]</color> Storage is Missing");
                break;
        }
    }
}
public enum InteractionType
{
    Storage
}