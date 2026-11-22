using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [Header("Need to work")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction inventoryToggleAction;
    private InputAction craftingToggleAction;
    private InputAction pauseToggleAction;
    private InputAction interactAction;

    [Header("Settings")]
    public LayerMask interactMask;
    private AudioSource audioSource;
    public float pickupRange = 3f;
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.8f;


    [Header("Things that you drag drog")]
    
    [Header("Pickup")]
    public GameObject PlayerUi;
    public GameObject hotbarObj; //TTT check for hotbar
    public GameObject inventorySlot;
    public GameObject container;
    public GameObject crafting;
    public GameObject revContainer;
    public Transform storageUI;
    public GameObject menus;

    private GameObject currentHandItem;

    [Header("Item Description")]
    public GameObject itemDescriptionParent;
    public Image itemDescriptionImage;
    public TextMeshProUGUI descriptionItemNameText;
    public TextMeshProUGUI ItemDescriptionText;

    [Header("Minigame Stuff")]
    public Image BasketballChargeFill;
    public GameObject BasketballCharge;
    public BasketballUI basketballUI;
    
    [Header("Crafting")]

    public List<Recipe> allRecipes = new();
    public Transform smallCraftingGrid;
    public GameObject craftingButtonPrefab;
    public GameObject craftingButtonPlusPrefab;
    public GameObject itemNeededUIPrefab;
    public Transform fullCraftingGrid;

    [Header("Slot Lists")]
    public Image dragIcon;

    private Material originalMaterial;
    private Renderer lookedAtRenderer = null;

    private int? equippedHotbarIndex = 0;


    private List<Slot> inventorySlots = new();
    private List<Slot> hotbarSlots = new();
    private List<Slot> allSlots = new();
    private List<Slot> storageUISlots = new();

    private Slot draggedSlot = null;
    private bool isDragging = false;
    private bool invFull;

    [Header("Player Stuff")]
    public Camera cam;
    public Player localPlayer;
    public Transform hand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        inventorySlots.AddRange(inventorySlot.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());
        storageUISlots.AddRange(storageUI.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);

        PopulateCraftingGrid();

        storageUI.gameObject.SetActive(false);

        inventoryToggleAction = inputActions.FindAction("Inventory");
        craftingToggleAction = inputActions.FindAction("Crafting");
        pauseToggleAction = inputActions.FindAction("Pause");
        interactAction = inputActions.FindAction("Interact");
    }

    void Start()
    {
        FixNulls();
    }

    void Update()
    {
        if (inventoryToggleAction.WasPressedThisFrame())
        {
            ToggleInventory();
        }
        else if (craftingToggleAction.WasPressedThisFrame())
        {
            container.SetActive(false);
            crafting.SetActive(!crafting.activeInHierarchy);
            PopulateCraftingGrid();
            bool isCraftingActive = crafting.activeInHierarchy;
            revContainer.SetActive(!isCraftingActive);
            ToggleMouse();
        }
        else if (pauseToggleAction.WasPressedThisFrame())
        {
            container.SetActive(false);
            menus.SetActive(!menus.activeInHierarchy);
            bool isPauseMenuActive = menus.activeInHierarchy;
            revContainer.SetActive(!isPauseMenuActive);
            ToggleMouse();
        }

        DetectLookedAtItem(); // highlight
        Pickup(); // item in local now

        StartDrag(); // click
        UpdateDragItemPosition(); // if click still
        EndDrag(); // not click

        HandleHotbarSelection(); // 1-8 keys
        UpdateHotbarOpacity(); // color of thingy

        UpdateItemDescription(); // just read
    }

    public void ToggleInventory()
    {
        crafting.SetActive(false);
        container.SetActive(!container.activeInHierarchy);
        PopulateCraftingGrid();
        bool isInventoryActive = container.activeInHierarchy;
        revContainer.SetActive(!isInventoryActive);
        ToggleMouse();

        Storage[] storages = FindObjectsByType<Storage>(FindObjectsSortMode.None);
        for (int i = 0; i < storages.Length; i++)
        {
            storages[i].CloseChest();
        }
        
    }

    private bool FullCraftingGridShown()
    {
        if (fullCraftingGrid.gameObject.activeInHierarchy) return true;
        if (smallCraftingGrid.gameObject.activeInHierarchy) return false;
        return false;
    }

    public void FixNulls()
    {
        if (audioSource == null && localPlayer != null) audioSource = localPlayer.playerAudioSource;
    }

    private void OnEnable()
    {
        inventoryToggleAction?.Enable();
        craftingToggleAction?.Enable();
        pauseToggleAction?.Enable();
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        inventoryToggleAction?.Disable();
        craftingToggleAction?.Disable();
        pauseToggleAction?.Disable();
        interactAction?.Disable();
    }

    public void ToggleMouse()
    {
        bool isInventoryActive = container.activeInHierarchy;
        bool isCraftingActive = crafting.activeInHierarchy;
        bool isPauseMenuActive = menus.activeInHierarchy;
        if (isInventoryActive || isCraftingActive || isPauseMenuActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MoveCamera.Instance.canLook = false;
            if (BasketballThrow.Instance != null)
                BasketballThrow.Instance.canThrow = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
            MoveCamera.Instance.canLook = true;
            if (BasketballThrow.Instance != null)
                BasketballThrow.Instance.canThrow = true;
        }
    }

    private void AddItem(ItemSO itemToAdd, int amount)
    {
        int remaining = amount;

        foreach(Slot slot in allSlots)
        {
            if(slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;
                    if (remaining <= 0)
                    {
                        PopulateCraftingGrid();
                        return;
                    }
                }
            }
        }

        foreach(Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;
                if (remaining <= 0)
                {
                    PopulateCraftingGrid();
                    return;
                }
            }
        }

        if (remaining > 0)
        {
            Debug.Log("Inventory Is Full, Cant Add " + remaining + " of " + itemToAdd.itemName);
        }
        PopulateCraftingGrid();
    }

    private void AddItemWithAmmo(ItemSO itemToAdd, int amount, int ammo, int maxAmmo)
    {
        int remaining = amount;

        if (maxAmmo == 0)
        {
            foreach (Slot slot in allSlots)
            {
                if (slot.HasItem() && slot.GetItem() == itemToAdd)
                {
                    int space = itemToAdd.maxStackSize - slot.GetAmount();
                    int add = Mathf.Min(space, remaining);

                    slot.SetItem(itemToAdd, slot.GetAmount() + add);

                    remaining -= add;

                    if (remaining <= 0)
                    {
                        PopulateCraftingGrid();
                        return;
                    }
                }
            }
        }

        foreach (Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                slot.SetItem(itemToAdd, remaining);

                slot.ammoAmount = ammo;
                slot.maxAmmo = maxAmmo;

                slot.UpdateSlot();
                if (remaining <= 0)
                {
                    PopulateCraftingGrid();
                }
                return;
            }
        }
        invFull = true;
        Debug.Log("<color=black>[Inventory]</color> Inventory full");
    }

    private void StartDrag()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.HasItem())
            {
                draggedSlot = hovered;
                isDragging = true;

                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color32(82, 82, 82, 128);
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            Slot hovered = GetHoveredSlot();
            if (hovered != null)
                HandleDrop(draggedSlot, hovered);
            else
                HandleDroppedItem(draggedSlot);
            dragIcon.enabled = false;
            draggedSlot = null;
            isDragging = false;
        }
    }

    private Slot GetHoveredSlot()
    {
        
        CleanupDestroyedStorageSlots();

        foreach (Slot s in allSlots)
            if (s != null && s.hovering)
                return s;

        foreach (Slot s in storageUISlots)
            if (s != null && s.hovering)
                return s;

        return null;
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to) 
            return;

        //Stacking
        if (to.HasItem() && to.GetItem() == from.GetItem() && from.maxAmmo == 0)
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if (space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());

                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if (from.GetAmount() <= 0) 
                    from.ClearSlot();

                return;
            }
        }

        //Swap
        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            int tempAmmo = to.ammoAmount;
            int tempMaxAmmo = to.maxAmmo;

            to.SetItem(from.GetItem(), from.GetAmount());
            to.ammoAmount = from.ammoAmount;
            to.maxAmmo = from.maxAmmo;

            from.SetItem(tempItem, tempAmount);
            from.ammoAmount = tempAmmo;
            from.maxAmmo = tempMaxAmmo;

            to.UpdateSlot();
            from.UpdateSlot();

            return;
        }

        to.SetItem(from.GetItem(), from.GetAmount());
        to.ammoAmount = from.ammoAmount;
        to.maxAmmo = from.maxAmmo;

        from.ClearSlot();

        to.UpdateSlot();
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging)
            dragIcon.transform.position = Mouse.current.position.ReadValue();
    }

    private void Pickup()
    {
        if (lookedAtRenderer != null && interactAction.WasPressedThisFrame())
        {
            if (lookedAtRenderer.TryGetComponent<Item>(out var item))
            {
                invFull = false;
                if (item.grabToHotbar)
                    AddToHotbarFirst(item.item, item.amount, item.ammoAmount, item.maxAmmo);
                else
                    AddItemWithAmmo(item.item, item.amount, item.ammoAmount, item.maxAmmo);

                if(!invFull)
                {
                    Destroy(item.gameObject);
                    EquipHandItem();
                }
            }
            if (lookedAtRenderer.TryGetComponent<InteractType>(out var interact))
            {
                interact.Interact(this);
            }
        }
    }

    public void PickupAuto(Renderer lookedAtRenderer)
    {
        if (lookedAtRenderer != null)
        {
            if (lookedAtRenderer.TryGetComponent<Item>(out var item))
            {
                invFull = false;
                if (item.grabToHotbar)
                    AddToHotbarFirst(item.item, item.amount, item.ammoAmount, item.maxAmmo);
                else
                    AddItemWithAmmo(item.item, item.amount, item.ammoAmount, item.maxAmmo);

                if(!invFull)
                {
                    Destroy(item.gameObject);
                    EquipHandItem();
                }
            }
            if (lookedAtRenderer.TryGetComponent<InteractType>(out var interact))
            {
                interact.Interact(this);
            }
        }
    }

    private void DetectLookedAtItem()   
    {
        if (lookedAtRenderer != null)
        {
            lookedAtRenderer.material = originalMaterial;
            lookedAtRenderer = null;
            originalMaterial = null;
        }
        if (cam == null) 
            return;

        Ray ray = new(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(cam.transform.position, cam.transform.forward * pickupRange, Color.red);

        RaycastHit[] hits = Physics.RaycastAll(ray, pickupRange);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            bool validLayer = (interactMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            bool validTag = hit.collider.CompareTag("IInteractable");

            if (!validLayer && !validTag)
                continue;

            if (hit.collider.TryGetComponent<Item>(out var item))
                if (item.TryGetComponent<Renderer>(out var rend))
                {
                    originalMaterial = rend.material;
                    //makes new material
                    // HDRP UNLIT
                    Material highlightMat = new(Shader.Find("HDRP/Unlit"))
                    {
                        color = item.highlightColor
                    };
                    rend.material = highlightMat;
                    lookedAtRenderer = rend;
                }

            if (hit.collider.TryGetComponent<InteractType>(out var interact))
                if (interact.TryGetComponent<Renderer>(out var rend))
                {
                    originalMaterial = rend.material;
                    lookedAtRenderer = rend;
                    interact.Hover(this);
                }

            break;
        }
    }

    private void UpdateHotbarOpacity()
    {
        for(int i = 0; i < hotbarSlots.Count; i++)
            if (hotbarSlots[i].TryGetComponent<Image>(out var icon))
                icon.color = (equippedHotbarIndex.HasValue && i == equippedHotbarIndex.Value) ? new Color(0/255f, 90/255f, 164/255f, equippedOpacity) : new Color(82/255f, 82/255f, 82/255f, normalOpacity);
    }

    private static readonly Key[] HotbarKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3,
        Key.Digit4, Key.Digit5, Key.Digit6,
        Key.Digit7, Key.Digit8, Key.Digit9
    };

    private void HandleHotbarSelection()
    {
        for(int i = 0; i < HotbarKeys.Length; i++)
            if(Keyboard.current[HotbarKeys[i]].wasPressedThisFrame)
            {
                if ((equippedHotbarIndex.HasValue && equippedHotbarIndex.Value == i) || (IsBlankSlot(hotbarSlots[i])))
                {
                    equippedHotbarIndex = null;
                    UpdateHotbarOpacity();
                    EquipHandItem();
                    return;
                }
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
            }
    }

    private bool IsBlankSlot(Slot slot)
    {
        return !slot.HasItem();
    }

    private void HandleDroppedItem(Slot from)
    {
        if (from == null || !from.HasItem())
            return;

        ItemSO itemSO = from.GetItem();
        GameObject prefab = itemSO.itemPrefab;

        if (prefab == null)
            return;

        Fragsurf.Movement.SurfCharacter player = gameObject.GetComponentInParent<Fragsurf.Movement.SurfCharacter>();

        Vector3 playerVelocity = Vector3.zero;

        if (player != null)
            playerVelocity = player.moveData.velocity;

        GameObject dropped = Instantiate(
            prefab,
            Camera.main.transform.position + Camera.main.transform.forward,
            Quaternion.identity
        );

        if (dropped.TryGetComponent<Item>(out var item))
        {
            item.item = itemSO;
            item.amount = from.GetAmount();
            item.ammoAmount = from.ammoAmount;
            item.maxAmmo = from.maxAmmo;
        }

        
        if (dropped.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = playerVelocity;
        }

        from.ClearSlot();
        EquipHandItem();
        PopulateCraftingGrid();
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null) Destroy(currentHandItem);

        if (!equippedHotbarIndex.HasValue)
            return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex.Value];
        if (!equippedSlot.HasItem())
            return;

        ItemSO item = equippedSlot.GetItem();
        if (item.handItemPrefab == null) 
            return;

        currentHandItem = Instantiate(item.handItemPrefab, hand);
        currentHandItem.transform.localRotation = Quaternion.identity;
        currentHandItem.SetActive(true);



        if (item.itemType == ItemSO.ItemType.Throwable)
        {
            if (item.name == "Basketball")
            {
                BasketballThrow.Instance.canThrow = true;
            }
        }


    }

    private void UpdateItemDescription()
    {
        Slot hoveredSlot = GetHoveredSlot();
        if(hoveredSlot != null)
        {
            ItemSO hoveredItem = hoveredSlot.GetItem();

            if (hoveredItem != null)
            {
                itemDescriptionParent.SetActive(true);
                itemDescriptionImage.sprite = hoveredItem.icon;
                ItemDescriptionText.text = hoveredItem.description;
                descriptionItemNameText.text = hoveredItem.name;
                return;
            }
        }
        itemDescriptionParent.SetActive(false);
    }

    private void PopulateCraftingGrid()
    {
        for (int i = smallCraftingGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(smallCraftingGrid.GetChild(i).gameObject);
        }

        for (int i = fullCraftingGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(fullCraftingGrid.GetChild(i).gameObject);
        }

        if (FullCraftingGridShown())
        {
            foreach(Recipe recipe in allRecipes)
            {
                GameObject buttonObject = Instantiate(craftingButtonPlusPrefab, fullCraftingGrid);
                Image image = buttonObject.transform.GetChild(0).GetComponent<Image>();
                image.sprite = recipe.result.icon;

                Button button = buttonObject.transform.GetChild(0).GetComponent<Button>();
                button.interactable = CanCraft(recipe);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Craft(recipe));

                foreach (Ingredient ingredient in recipe.ingredients)
                {
                    GameObject neededItem = Instantiate(itemNeededUIPrefab, buttonObject.transform.GetChild(1));
                    neededItem.GetComponent<Image>().sprite = ingredient.item.icon;
                    neededItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + ingredient.amount.ToString();
                }
            }
        }
        else
        {
            foreach(Recipe recipe in allRecipes)
            {
                GameObject buttonObject = Instantiate(craftingButtonPrefab, smallCraftingGrid);
                Image image = buttonObject.transform.GetChild(0).GetComponent<Image>();
                image.sprite = recipe.result.icon;

                Button button = buttonObject.transform.GetChild(0).GetComponent<Button>();
                button.interactable = CanCraft(recipe);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Craft(recipe));
            }
        }
    }

    public void Craft(Recipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("<color=black>[Inventory]</color> Cant Craft: " + recipe.name);
            return;
        }
        Debug.Log("<color=black>[Inventory]</color> Crafted: " + recipe.name);
        ConsumeIngredients(recipe);
        AddItem(recipe.result, recipe.resultAmount);

        PopulateCraftingGrid();
    }

    private void ConsumeIngredients(Recipe recipe)
    {
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            int remaining = ingredient.amount;

            foreach(Slot slot in allSlots)
            {
                if (!slot.HasItem())
                    continue;
                if (slot.GetItem() != ingredient.item)
                    continue;

                int take = Mathf.Min(slot.GetAmount(), remaining);
                slot.SetItem(slot.GetItem(), slot.GetAmount() - take);

                if (slot.GetAmount() <= 0)
                    slot.ClearSlot();
                
                remaining -= take;
                if (remaining <= 0)
                    break;
            }
        }
    }

    private bool CanCraft(Recipe recipe)
    {
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            int totalFound = 0;

            foreach (Slot slot in allSlots)
            {
                if (slot.HasItem() && slot.GetItem() == ingredient.item)
                {
                    totalFound += slot.GetAmount();
                }
            }

            if (totalFound < ingredient.amount)
                return false;
        }
        return true;
    }

    private void AddToHotbarFirst(ItemSO itemToAdd, int amount, int ammo, int maxAmmo)
    {
        int remaining = amount;

        // 1. Try stacking in hotbar
        foreach (Slot slot in hotbarSlots)
        {
            if (slot.HasItem() && slot.GetItem() == itemToAdd && maxAmmo == 0)
            {
                int space = itemToAdd.maxStackSize - slot.GetAmount();
                int add = Mathf.Min(space, remaining);

                slot.SetItem(itemToAdd, slot.GetAmount() + add);
                remaining -= add;

                if (remaining <= 0)
                {
                    PopulateCraftingGrid();
                    return;
                }
            }
        }

        // 2. Try empty hotbar slots
        foreach (Slot slot in hotbarSlots)
        {
            if (!slot.HasItem())
            {
                slot.SetItem(itemToAdd, remaining);
                slot.ammoAmount = ammo;
                slot.maxAmmo = maxAmmo;
                slot.UpdateSlot();
                PopulateCraftingGrid();
                return;
            }
        }

        // 3. Fallback to inventory
        AddItemWithAmmo(itemToAdd, remaining, ammo, maxAmmo);
    }

    public void RegisterStorageSlot(Slot slot)
    {
        if (!storageUISlots.Contains(slot))
        {
            storageUISlots.Add(slot);
        }
    }

    public void UnregisterStorageSlot(Slot slot)
    {
        if (slot == null)
            return;

        storageUISlots.Remove(slot);
    }

    private void CleanupDestroyedStorageSlots()
    {
        storageUISlots.RemoveAll(slot => slot == null);
    }

    public void UseItem(GameObject itemUsed)
    {
        if (!equippedHotbarIndex.HasValue) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex.Value];

        if (!equippedSlot.HasItem()) return;

        int newAmount = equippedSlot.GetAmount() - 1;

        if (newAmount <= 0)
        {
            equippedSlot.ClearSlot(); // BUG: fix later
            equippedHotbarIndex = null;
        }
        else
            equippedSlot.SetItem(equippedSlot.GetItem(), newAmount);

        EquipHandItem();
        UpdateHotbarOpacity();

        Destroy(itemUsed);
    }

    private void PlayAudio(AudioClip clip, float pitchRangeMin, float pitchRangeMax)
    {
        audioSource.pitch = UnityEngine.Random.Range(pitchRangeMin, pitchRangeMax);
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }
}
