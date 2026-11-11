using UnityEngine;

public class FallFix : MonoBehaviour
{
    public GameObject playerRespawnPoint;

    public GameObject basketballRespawnPoint;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Transform player = other.transform;
            while (player.parent != null)
            {
                player = player.parent;
            }
            // player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; //Cant do cuz player is kinematic
            player.position = playerRespawnPoint.transform.position;
        }

        else if (other.gameObject.CompareTag("MinigameObject"))
        {
            if (other.transform.GetComponent<Item>().item.itemName == "Basketball")
            {
                other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                other.transform.position = basketballRespawnPoint.transform.position;
            }
            else
            {
                print("<color=black>[Fall Fix]</color> Item not found: " + other.transform.GetComponent<Item>().item.itemName);
            }
        }
        else
        {
            print("<color=black>[Fall Fix]</color> Object not found: " + other.gameObject.name);
        }
    }
    
}
