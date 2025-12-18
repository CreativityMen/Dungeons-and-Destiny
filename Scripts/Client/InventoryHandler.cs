/*

*/

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static InteractHandler;
using static UnityEditor.Progress;

public class InventoryHandler : MonoBehaviour
{
    // Properties
    [Header("Properties")]
    [SerializeField]
    public List<GameObject> slotList = new List<GameObject>();
    private float maxSlots = 5f;

    // Other
    public GameObject playerRotation;
    public GameObject inventoryParent;

    private GameObject InventoryUI;
    private GameObject SlotsUI;

    // Components
    public AudioSource audioSource;

    // Scripts
    [Header("Scripts")]
    public InputManager inputManager;

    // Inputs
    private List<ActionType> actions;

    // Placeholder Variables
    public GameObject currentItem;
    public GameObject lastItem;

    // Data
    [SerializeField] private ItemData itemData;

    private Dictionary<GameObject, InteractableType> savedItems = new Dictionary<GameObject, InteractableType>();

    public void AddItemToPlayer(GameObject item)
    {
        if (item == null || playerRotation == null) return;
        if (!inventoryParent)
        {
            Debug.LogWarning("InventoryParent is not set or doesn't exist");
            return;
        }

        item.transform.position = inventoryParent.transform.position;
        item.transform.parent = inventoryParent.transform;

        item.SetActive(false);
        item.GetComponent<Collider>().enabled = false;
    }

    public void AddItemToHotbar(GameObject item)
    {
        if (!inventoryParent) return;

        itemData = Resources.Load<ItemData>("ItemData/" + item.name);

        if (itemData == null)
        {
            itemData = Resources.Load<ItemData>("ItemData/Sword");
        }

        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] != null)
            {
                Transform slot = SlotsUI.transform.Find((i + 1).ToString());

                GameObject itemIcon = Instantiate(itemData.prefab, slot);
                itemIcon.transform.position = itemIcon.transform.parent.transform.position;

                itemIcon.GetComponent<Image>().sprite = itemData.itemIcon;
            }
        }
    }

    public void SortItem(GameObject item, string className, InteractableType interactableType)
    {
        if (slotList.Count == maxSlots) return;

        savedItems.Add(item, interactableType);
        audioSource = item.AddComponent<AudioSource>();

        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] == null)
            {
                slotList[i] = item;

                AddItemToPlayer(item);
                AddItemToHotbar(item);
                return;
            }
        }

        slotList.Add(item);

        AddItemToPlayer(item);
        AddItemToHotbar(item);
    }

    public void DropItem()
    {
        if (!currentItem) return;

        InteractHandler _interactHandler = currentItem.AddComponent<InteractHandler>();
        Destroy(currentItem.GetComponent(currentItem.name));

        currentItem.transform.parent = GameObject.Find("Environment").transform;
        currentItem.GetComponent<Collider>().enabled = true;

        if (savedItems.ContainsKey(currentItem))
        {
            _interactHandler.interactableType = savedItems[currentItem];
            _interactHandler.ClassName = currentItem.name;

            savedItems.Remove(currentItem);
        }

        Destroy(currentItem.GetComponent<AudioSource>());

        for (int i = 0; i < slotList.Count; i++)
        {
            if (slotList[i] == currentItem)
            {
                Transform slot = SlotsUI.transform.Find((i + 1).ToString());

                foreach (Transform child in slot)
                {
                    Destroy(child.gameObject);
                }

                slotList[i] = null;
            }
        }


        currentItem = null;
    }

    public void UnequipItem()
    {
        currentItem.SetActive(false);
        currentItem = null;
    }

    public void EquipItem()
    {
        itemData = Resources.Load<ItemData>("ItemData/" + currentItem.name);

        audioSource.clip = itemData.itemEquip;
        audioSource.Play();

        currentItem.SetActive(true);
    }

    public void DetectInput()
    {
        // Gets the selected slot int from InputManager.cs
        int slot = inputManager.selectedSlot;

        foreach (var action in inputManager.inputList)
        {
            if (action.actionName == "Drop" && action.hasPressed)
            {
                DropItem();
            }
        }

        // If less than 0, or greater than or equal to the length of the list, return
        if (slot < 0 || slot >= slotList.Count)
            return;

        // Sets the current item
        GameObject selectedItem = slotList[slot];

        if (currentItem == selectedItem && currentItem.activeSelf)
        {
            UnequipItem();
            currentItem = null;
        }
        else
        {
            if (currentItem != null && currentItem.activeSelf)
            {
                UnequipItem();
            }

            currentItem = selectedItem;
            EquipItem();
        }

        // Reset after equipping so that it doesn't equip the item again every frame
        inputManager.selectedSlot = -1;
        Debug.Log(currentItem);
    }

    private void Start()
    {
        playerRotation = GameObject.FindGameObjectWithTag("Player").transform.parent.Find("PlayerRotation").gameObject;
        inventoryParent = playerRotation.transform.Find("Inventory").gameObject;

        Transform hud = GameObject.Find("HUD").transform;

        if (hud != null)
        {
            InventoryUI = hud.Find("Inventory").gameObject;
            SlotsUI = InventoryUI.transform.Find("Hotbar/HotbarBackground/Slots").gameObject;
        }


        inputManager = GetComponent<InputManager>();
        actions = inputManager.inputList;
    }

    private void Update()
    {
        if (inputManager == null) return;

        DetectInput();
    }
}
