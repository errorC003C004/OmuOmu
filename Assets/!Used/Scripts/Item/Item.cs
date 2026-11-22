using UnityEngine;
public class Item : MonoBehaviour
{
    public ItemSO item;
    public int amount = 1;
    public Color highlightColor;
    public bool autoPickup;

    void Start()
    {
        if (autoPickup)
        {
            GetComponentInChildren<ItemAutoPickup>().item = this;
        }
    }

    [Header("If Weapon")]
    public int ammoAmount;
    public int maxAmmo;
    public bool grabToHotbar;
}