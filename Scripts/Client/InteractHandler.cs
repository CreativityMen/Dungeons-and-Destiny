/*
    Title: InteractHandler.cs
    Author: Afonso Marques
    
    Description: Main script responsible for handling
    the way the player interacts with any type of object
    that is interactable.
 */

using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractHandler : MonoBehaviour
{
    // Properties
    [Header("Main Properties")]
    [SerializeField] private float threshold = 5f;
    public static bool isInteracting;

    [Header("Object Properties")]
    public string ClassName;
    public string NPCName;

    public bool CanTalk;

    public List<string> Messages = new List<string>();

    // Enum Types / Current Enum Type
    public enum InteractableType
    {
        NPC,
        Item
    }
    public InteractableType interactableType;

    // Scripts
    public InputManager inputManager;
    public MotorHandler motorHandler;
    public InventoryHandler inventoryHandler;

    // Placeholder Variables
    private GameObject thisObject;
    private GameObject playerObject;

    private float timer = 0.0f;

    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player").gameObject;
        inputManager = GameObject.Find("Client").GetComponent<InputManager>();
        inventoryHandler = GameObject.Find("Client").GetComponent<InventoryHandler>();
        motorHandler = playerObject.GetComponent<MotorHandler>();
        thisObject = this.gameObject;
    }

    // Function responsible for adding the object's class the player has interacted to the client based on the type
    public void AddTypeClassAsComponent()
    {
        if (isInteracting) return;
        if (CanTalk && Messages.Count == 0)
        {
            Debug.LogWarning("CanTalk is enabled but there are no messages in the list");
            return;
        }
        
        isInteracting = true;

        // Inititates whatever functionality is needed from a class for the type of the object
        if (ClassName == "DialogueHandler")
        {
            Type type = Type.GetType(ClassName + ", Assembly-CSharp");
            thisObject.AddComponent(type);
        }
        else
        {
            ClassName = gameObject.name;

            Debug.Log(ClassName);
            Type type = Type.GetType(ClassName + ", Assembly-CSharp");
            thisObject.AddComponent(type);

            inventoryHandler.SortItem(thisObject, ClassName, interactableType);

            Destroy(gameObject.GetComponent<InteractHandler>());
            isInteracting = false;
        }
    }

    public void CheckForDistance()
    {
        Vector3 playerPos = playerObject.transform.position;
        Vector3 thisObjectPos = thisObject.transform.position;

        float distance = Vector3.Distance(playerPos, thisObjectPos);

        if (distance >= threshold || isInteracting) return;

        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            foreach (var action in inputManager.inputList)
            {
                if (action.actionName == "Interact" && action.hasPressed)
                {
                    AddTypeClassAsComponent();
                    timer = 0f;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (thisObject.tag != "Interactable")
        {
            Debug.LogWarning("Object is missing Interactable tag"); 
            return;
        }

        CheckForDistance();
    }
}
