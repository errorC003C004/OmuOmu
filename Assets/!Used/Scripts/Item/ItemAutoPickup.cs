using UnityEngine;

public class ItemAutoPickup : MonoBehaviour
{
    public Item item;
    public float timeUntilPickup;
    private bool canPickup = false;
    private void OnTriggerEnter(Collider other)
    {
        if (canPickup && (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player")))
        {
            print("<color=black>[Item Auto Pickup]</color> Touched Player With auto Pickup!");
            if (item != null)
            {
                Inventory.Instance.PickupAuto(item.GetComponent<Renderer>());
            }
            canPickup = false;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForPickup());
    }
    
    private System.Collections.IEnumerator WaitForPickup()
    {
        yield return new WaitForSeconds(timeUntilPickup);
        canPickup = true;
    }
}
