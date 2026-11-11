using UnityEngine;

public class ColliderHelper : MonoBehaviour
{
    public ColliderType colliderType;

    private void Start()
    {
        switch (colliderType)
        {
            case ColliderType.Basketball:
                //Debug.Log("<color=black>[Collider Helper]</color> Basketball game initialized for " + gameObject.name);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MinigameObject") && colliderType.ToString() != null && colliderType.ToString() != "")
        {
            if (colliderType == ColliderType.Basketball)
            {
                Inventory.Instance.basketballUI.AddScore(1);
                if (!Inventory.Instance.basketballUI.isTimerRunning)
                {
                    Inventory.Instance.basketballUI.StartTimer();
                }
            }
            Debug.Log("<color=black>[Collider Helper]</color> Collided with " + other.name + " in game type " + colliderType.ToString());
        }
    }

}
public enum ColliderType
{
    Basketball
}