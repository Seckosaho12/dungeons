using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCursor : MonoBehaviour
{

    public static VirtualCursor Instance { get; private set; } // Singleton instance
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Set the singleton instance
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }
    [SerializeField] private float radiusAroundPlayer = 1f; // Radius around the player to keep the cursor within
    [SerializeField] private PlayerControls playerControls; // Reference to the PlayerControls script
    [SerializeField] private Transform playerTransform; // Reference to the player's transform
    [SerializeField] private Transform cursorTransform; // Reference to the cursor's transform

    [SerializeField] private Vector2 lookVector; // Vector to store the look direction
    [SerializeField] private Vector2 mousePosition; // Vector to store the mouse position

    public bool isUsingController = false; // Flag to check if using controller

    private void OnEnable()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls = new PlayerControls();
        playerControls.Disable();
        playerControls.Combat.Disable();
        playerControls.Inventory.Disable();
        playerControls.Movement.Disable();
    }
    void Update()
    {
        mousePosition = Input.mousePosition + new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }
    void Start()
    {
        cursorTransform.position = playerTransform.position + new Vector3(1, 0, 0) * radiusAroundPlayer; // Initialize cursor position
        playerControls.Movement.LookController.performed += ctx => MoveCursor(ctx.ReadValue<Vector2>());
        playerControls.Movement.LookMouse.performed += ctx =>
        {
            cursorTransform.gameObject.SetActive(false); // Hide the cursor when using mouse look
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isUsingController = false; // Set flag to false when using mouse look
        };
    }

    private void MoveCursor(Vector2 lookVector)
    {
        if (lookVector == Vector2.zero)
            return;

        this.lookVector = lookVector; // Update the look vector with the input value
        cursorTransform.gameObject.SetActive(true); // Show the cursor when moving
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
        isUsingController = true; // Set flag to true when using controller
        // Calculate the new position of the cursor based on the player's position and the input vector
        Vector3 newPosition = playerTransform.position + new Vector3(lookVector.x, lookVector.y, 0) * radiusAroundPlayer;

        // Clamp the cursor's position to be within the specified radius around the player
        float distanceFromPlayer = Vector3.Distance(playerTransform.position, newPosition);
        if (distanceFromPlayer > radiusAroundPlayer)
        {
            Vector3 direction = (newPosition - playerTransform.position).normalized;
            newPosition = playerTransform.position + direction * radiusAroundPlayer;
        }

        cursorTransform.position = newPosition;
    }

    public Vector2 GetLookVector()
    {
        return lookVector;
    }
}
