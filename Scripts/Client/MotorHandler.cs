/*
    Title: MotorHandler.cs
    Author: Afonso Marques
    
    Description: The main movement handler for the player.
    Detected input gets sent from InputManager.cs for movement
    functionality.
 */

using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.U2D;

[System.Serializable]
public class RayBool
{
    public string rayName;
    public bool value;

    public RayBool(string rayName, bool value)
    {
        this.rayName = rayName;
        this.value = value;
    }
}

public class MotorHandler : MonoBehaviour
{
    // Player
    [Header("Player")]
    public Transform player;
    public Transform playerParent;
    public Transform playerRotation;

    // Player Values
    [Header("Movement Values")]
    public float walkSpeed = 16.0f;

    // Player Properties
    [Header("Vector3's")]
    public Vector3 moveInputVector = Vector3.zero;
    private Vector3 currentSetPosition;
    public Vector3 lastPosition;
    [SerializeField] private Vector3 _moveDirection = Vector3.zero;
    [SerializeField] private Vector3 _cameraMoveDirection = Vector3.zero;

    // Scripts
    [Header("Scripts")]
    public InputManager inputManager;
    public StatusDisplayer statusDisplayer;

    // Required Scripts As Components
    public List<Type> requiredComponents = new()
    {
        typeof(HealthManager)
    };
    
    // Enums
    public enum States
    {
        CanMove,
        SetPosition
    }

    public States currentState;

    [Header("Rigidbody Properties")]
    public Rigidbody rigidBody;

    // Other
    [Header("Other")]
    public Camera currentCamera;
    private LayerMask wallLayer;
    private GameObject sprite;
    private Animator animator;

    // Rays
    private GameObject RaycastsObject;
    private GameObject CheckWallObject;

    [SerializeField]
    public Dictionary<string, RayBool> raycasts = new()
    {
        { "CheckForWall", new RayBool("CheckForWall", false) }
    };

    // Function responsible for managing sprite animation based on the direction of the player
    void SpriteManager()
    {
        if (!sprite) return;

        // Stationary
        if (_moveDirection.sqrMagnitude == 0)
        {
            animator.Play("Idle");
        }

        // Linear Movement
        if (_moveDirection.z == 0)
        {
            if (_moveDirection.x > 0)
            {
                animator.Play("Running Right");
            }
            else if (_moveDirection.x < 0)
            {
                animator.Play("Running Left");
            }
        }

        if (_moveDirection.x == 0)
        {
            if (_moveDirection.z > 0)
            {
                animator.Play("Running Up");
            }
            else if (_moveDirection.z < 0)
            {
                animator.Play("Running Down");
            }
        }

        // Diagonal Movement
        if (_moveDirection.z > 0)
        {
            if (_moveDirection.x > 0)
            {
                animator.Play("Running Right Up");
            }
            else if (_moveDirection.x < 0)
            {
                animator.Play("Running Left Up");
            }
        }
        else if (_moveDirection.z < 0)
        {
            if (_moveDirection.x > 0)
            {
                animator.Play("Running Right Down");
            }
            else if (_moveDirection.x < 0)
            {
                animator.Play("Running Left Down");
            }
        }
        
        sprite.transform.LookAt(currentCamera.transform.position);
    }

    // Function responsible for rotating the other game object (PlayerRotation) that only handles rotation
    public void RotateTowards(Vector3 direction)
    {
        bool isMoving = (transform.position - lastPosition).sqrMagnitude > 0.0001f;

        if (!isMoving) return;

        playerRotation.transform.position = Vector3.Lerp(
            playerRotation.transform.position,
            player.transform.position,
            20f * Time.deltaTime
        );


        Quaternion targetRotation = Quaternion.LookRotation(direction);
        playerRotation.rotation = Quaternion.Slerp(
            playerRotation.rotation,
            targetRotation,
            10f * Time.deltaTime
        );
    }

    // Main function responsible for handling the movement of the player
    public void MovementManager()
    {
        if (inputManager == null) return;

        // Player and Camera Vector3's based on their look vectors
        
        //-- Tried to just use the camera's look vectors to handle both the movement and rotation,
        //-- But when trying to use 
        Vector3 cameraForward = new Vector3(currentCamera.transform.forward.x, 0, currentCamera.transform.forward.z);
        Vector3 cameraRight = new Vector3(currentCamera.transform.right.x, 0, currentCamera.transform.right.z);

        Vector3 playerForward = new Vector3(player.transform.forward.x, 0, player.transform.forward.z);
        Vector3 playerRight = new Vector3(player.transform.right.x, 0, player.transform.right.z);

        // Gets the input vector from InputManager.cs to know in which direction the player wants to move to
        Vector2 input = inputManager.moveVectorInput;

        // Vector3 angle movement variables that will act accordingly to where the camera is facing
        (Vector3 forward, Vector3 right) = (playerForward, playerRight);

        // Normalizes both vectors
        forward.Normalize();
        right.Normalize();

        // Move Direction
        Vector3 moveDirection = 
            playerForward * input.y
            + playerRight * input.x;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Camera MoveDirection
        Vector3 cameraMoveDirection =
            cameraForward * input.y
            + cameraRight * input.x;

        moveDirection = moveDirection.normalized;

        _moveDirection = moveDirection;
        _cameraMoveDirection = cameraMoveDirection;

        if (statusDisplayer)
        {
            if (moveDirection.sqrMagnitude > 0)
            {
                statusDisplayer.movementStatus = "Moving";
            }
            else
            {
                statusDisplayer.movementStatus = "Idle";
                lastPosition = player.position;
            }
        }

        // Keeps the current Y velocity
        Vector3 movementVector = new Vector3(
            moveDirection.x * walkSpeed,
            rigidBody.linearVelocity.y,
            moveDirection.z * walkSpeed);

        rigidBody.linearVelocity = movementVector;
    }

    void FetchClasses()
    {
        foreach (var _class in requiredComponents)
        {
            gameObject.AddComponent(_class);
        }
    }

    public void Start()
    {
        player = transform;
        playerParent = transform.parent;
        playerRotation = playerParent.transform.Find("PlayerRotation");

        sprite = player.Find("Visuals/playersprite").gameObject;
        animator = sprite.GetComponent<Animator>();

        if (GameObject.Find("StatusDisplayer"))
        {
            statusDisplayer = GameObject.Find("StatusDisplayer").GetComponent<StatusDisplayer>();
        }

        RaycastsObject = player.Find("Raycasts").gameObject;
        CheckWallObject = RaycastsObject.transform.Find("CheckWall").gameObject;
        
        wallLayer = LayerMask.GetMask("Wall");

        currentCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        inputManager = GameObject.Find("Client").GetComponent<InputManager>();
        rigidBody = GetComponent<Rigidbody>();

        // Spawn Point
        SetPositionState(GameObject.Find("StarterPoint").transform.position);
        
        FetchClasses();
        SetPosition();
    }

    public void SetPositionState(Vector3 position)
    {
        if (position != null)
        {
            currentState = States.SetPosition;
            currentSetPosition = position;
        }
        else
        {
            Debug.LogWarning("Player has no starter point");
            currentState = States.CanMove;
        }
    }

    public void SetPosition()
    {
        float tolerance = 1f;
        float sqrTolerance = tolerance * tolerance;

        if ((player.position - currentSetPosition).sqrMagnitude < sqrTolerance)
        {
            currentState = States.CanMove;
        }

        player.position = currentSetPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentState == States.CanMove)
        {
            MovementManager();
            SpriteManager();
        }
        else
        {
            SetPosition();
        }
    }

    void LateUpdate()
    {
        RotateTowards(_cameraMoveDirection);
    }
}
