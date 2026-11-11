using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private ItemSO heldItem;
    private int itemAmount;
    public int ammoAmount;
    public int maxAmmo;

    private Image iconImage;
    private TextMeshProUGUI itemCountTxt;
    private TextMeshProUGUI ammoAmountTxt;

    private void Awake()
    {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        itemCountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        ammoAmountTxt = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (iconImage == null)
            iconImage = transform.GetChild(0).GetComponent<Image>();

        if (itemCountTxt == null)
            itemCountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        if (ammoAmountTxt == null && transform.childCount > 2)
            ammoAmountTxt = transform.GetChild(2).GetComponent<TextMeshProUGUI>();


        if (heldItem != null)
        {
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = heldItem.icon;
            }

            if (itemCountTxt != null)
                itemCountTxt.text = itemAmount.ToString();

            if (ammoAmountTxt != null)
                ammoAmountTxt.text = ammoAmount + "/" + maxAmmo;
        }
        else
        {
            if (iconImage != null)
                iconImage.enabled = false;

            if (itemCountTxt != null)
                itemCountTxt.text = "";

            if (ammoAmountTxt != null)
                ammoAmountTxt.text = "";
        }
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;

        ammoAmount = 0;
        maxAmmo = 0;

        UpdateSlot();
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void SetAmmo(int current, int max)
    {
        ammoAmount = current;
        maxAmmo = max;
        UpdateSlot();
    }
}
