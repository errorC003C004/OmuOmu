using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StorageItem
{
    public ItemSO item;
    public int amount;
    public int ammoAmount;
}

public class Storage : MonoBehaviour
{
    [SerializeField] private bool initialized = false;

    public int storageSize = 24;
    public StorageItem[] itemsToRandomlyAdd;
    public StorageItem[] storedItems;

    [Header("Storage UI")]
    [SerializeField] private GameObject slotPrefab;

    // Shared UI panel from Inventory.
    private static GameObject storageUI;

    // ONLY contains the slots for this storage while it is open.
    private Slot[] slots;

    private bool isOpen;

    void Start()
    {
        // Initialize storage data.
        if (!initialized)
        {
            storedItems = new StorageItem[storageSize];

            for (int i = 0; i < storageSize; i++)
            {
                storedItems[i] = new StorageItem();
            }

            initialized = true;
        }

        // Get the Storage UI.
        if (storageUI == null)
        {
            if (Inventory.Instance == null)
            {
                Debug.LogError("Storage: Inventory.Instance is null.");
                return;
            }

            if (Inventory.Instance.storageUI == null)
            {
                Debug.LogError("Storage: Inventory.storageUI is null.");
                return;
            }

            storageUI = Inventory.Instance.storageUI.gameObject;
        }

        // Make sure the Slot prefab is assigned.
        if (slotPrefab == null)
        {
            Debug.LogError("Storage: Slot Prefab is not assigned.");
            return;
        }

        // No slots are created here anymore.
        // They are created only when this storage is opened.

        // Hide storage UI until the storage is opened.
        storageUI.SetActive(false);

        // Add randomized starting items.
        AddItemToStorage(itemsToRandomlyAdd);
    }

    private void CreateSlots()
    {
        // Make sure we start with no slots.
        DestroySlots();

        slots = new Slot[storedItems.Length];

        for (int i = 0; i < storedItems.Length; i++)
        {
            GameObject newSlotObject = Instantiate(
                slotPrefab,
                storageUI.transform,
                false
            );

            newSlotObject.name = "Storage Slot " + i;

            Slot newSlot = newSlotObject.GetComponent<Slot>();

            if (newSlot == null)
            {
                Debug.LogError(
                    "Storage: Slot prefab does not have a Slot component."
                );

                Destroy(newSlotObject);
                continue;
            }

            slots[i] = newSlot;

            // Inventory needs to know about dynamically-created slots
            // because its drag system checks registered storage slots.
            Inventory.Instance.RegisterStorageSlot(newSlot);
        }
    }

    private void DestroySlots()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                Inventory.Instance.UnregisterStorageSlot(slots[i]);
                Destroy(slots[i].gameObject);
            }
        }

        slots = null;
    }

    public void Interact()
    {
        if (!isOpen)
        {
            OpenChest();
        }
        else
        {
            CloseChest();
        }
    }

    private void OpenChest()
    {
        // Create this storage's UI slots.
        CreateSlots();

        // Open the inventory UI.
        Inventory.Instance.ToggleInventory();

        isOpen = true;
        storageUI.SetActive(true);

        // Fill the newly-created slots from saved storage data.
        for (int i = 0; i < storedItems.Length; i++)
        {
            StorageItem data = storedItems[i];

            if (data.item != null)
            {
                slots[i].SetItem(
                    data.item,
                    data.amount
                );
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void CloseChest()
    {
        if (!isOpen)
            return;

        isOpen = false;

        // Save the current UI contents back into this storage.
        SaveSlots();

        // The UI slots are no longer needed.
        DestroySlots();

        // Hide the shared storage UI.
        storageUI.SetActive(false);
    }

    private void SaveSlots()
    {
        if (slots == null)
            return;

        // Make sure the stored data array matches the number
        // of currently displayed slots.
        if (storedItems.Length != slots.Length)
        {
            System.Array.Resize(
                ref storedItems,
                slots.Length
            );

            for (int i = 0; i < storedItems.Length; i++)
            {
                if (storedItems[i] == null)
                {
                    storedItems[i] = new StorageItem();
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].HasItem())
            {
                storedItems[i].item = slots[i].GetItem();
                storedItems[i].amount = slots[i].GetAmount();

                // If your Slot class has a GetAmmoAmount() method,
                // restore this line:
                //
                // storedItems[i].ammoAmount = slots[i].GetAmmoAmount();
            }
            else
            {
                storedItems[i].item = null;
                storedItems[i].amount = 0;
                storedItems[i].ammoAmount = 0;
            }
        }

        storageSize = storedItems.Length;
    }

    public void AddItemToStorage(StorageItem[] itemsToAdd)
    {
        if (itemsToAdd == null || itemsToAdd.Length == 0)
            return;

        foreach (StorageItem itemToAdd in itemsToAdd)
        {
            if (itemToAdd == null || itemToAdd.item == null)
                continue;

            // Find all currently empty storage slots.
            List<int> emptySlots = new List<int>();

            for (int i = 0; i < storedItems.Length; i++)
            {
                if (storedItems[i] == null)
                {
                    storedItems[i] = new StorageItem();
                }

                if (storedItems[i].item == null)
                {
                    emptySlots.Add(i);
                }
            }

            // No empty slots.
            // Dynamically expand the storage data.
            if (emptySlots.Count == 0)
            {
                int oldSize = storedItems.Length;

                System.Array.Resize(
                    ref storedItems,
                    oldSize + 1
                );

                storedItems[oldSize] = new StorageItem();

                storageSize = storedItems.Length;

                // If this storage is currently open, create
                // a new UI slot for the newly-added storage slot.
                if (isOpen && slots != null)
                {
                    GameObject newSlotObject = Instantiate(
                        slotPrefab,
                        storageUI.transform,
                        false
                    );

                    newSlotObject.name = "Storage Slot " + oldSize;

                    Slot newSlot = newSlotObject.GetComponent<Slot>();

                    if (newSlot == null)
                    {
                        Debug.LogError(
                            "Storage: The Slot prefab does not have a Slot component."
                        );

                        Destroy(newSlotObject);
                        continue;
                    }

                    System.Array.Resize(
                        ref slots,
                        slots.Length + 1
                    );

                    slots[oldSize] = newSlot;

                    Inventory.Instance.RegisterStorageSlot(newSlot);
                }

                emptySlots.Add(oldSize);
            }

            // Pick a random empty slot.
            int randomIndex = Random.Range(
                0,
                emptySlots.Count
            );

            int slotIndex = emptySlots[randomIndex];

            // Put the item into the saved storage data.
            storedItems[slotIndex] = new StorageItem
            {
                item = itemToAdd.item,
                amount = itemToAdd.amount,
                ammoAmount = itemToAdd.ammoAmount
            };

            // If the storage is currently open, update the UI slot too.
            if (isOpen &&
                slots != null &&
                slotIndex < slots.Length &&
                slots[slotIndex] != null)
            {
                slots[slotIndex].SetItem(
                    itemToAdd.item,
                    itemToAdd.amount
                );
            }
        }
    }
}