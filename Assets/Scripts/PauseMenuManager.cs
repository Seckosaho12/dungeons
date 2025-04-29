using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Add this namespace

public class PauseMenuManager : Singleton<PauseMenuManager>
{
    [SerializeField] private GameObject pauseMenuUI; // Assign the pause menu UI in the Inspector
    private bool isPaused = false;
    private bool gameEnded = false;
    [SerializeField] private EventSystem eventSystem; // Reference to the EventSystem

    void Start()
    {
        // Ensure the pause menu is hidden at the start
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Pause Menu UI is not assigned in the Inspector.");
        }

        // Get reference to EventSystem if not assigned
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }
    }

    private void HidePauseMenuOnSceneLoad()
    {
        if (pauseMenuUI != null && isPaused)
        {
            pauseMenuUI.SetActive(false); // Hide the pause menu when a new scene is loaded
            Time.timeScale = 1f; // Resume game time
            isPaused = false;
        }
    }

    void Update()
    {
        // Check if the game is ended and prevent any input
        if (gameEnded)
        {
            return; // Exit Update if the game has ended
        }
        // Toggle pause menu when ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        Time.timeScale = 1f; // Resume game time
        PlayerController.Instance.enabled = true; // Enable player controls
        ActiveInventory.Instance.enabled = true; // Enable inventory controls
        isPaused = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Pause game time
        PlayerController.Instance.enabled = false; // Disable player controls
        ActiveInventory.Instance.enabled = false; // Disable inventory controls
        isPaused = true;

        // // If we have an event system, make sure it can process events when time is stopped
        // if (eventSystem != null)
        // {
        //     eventSystem.sendNavigationEvents = true;
        // }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Ensure game time is resumed before switching scenes
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single); // Replace "MainMenu" with the name of your main menu scene
    }

    public void QuitToDesktop()
    {
        Application.Quit(); // Quit the application
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#endif
    }

    public bool IsPaused()
    {
        return isPaused; // Return the current pause state
    }

    public void EndGame()
    {
        Time.timeScale = 0f; // Pause game time
        PlayerController.Instance.enabled = false; // Disable player controls
        ActiveInventory.Instance.enabled = false; // Disable inventory controls
        gameEnded = true; // Set game ended state
        isPaused = true; // Set pause state to true
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume game time
        gameEnded = false; // Reset game ended state
        isPaused = false; // Reset pause state
        PlayerPrefs.SetInt("Restarted", 1);
        PlayerPrefs.Save(); // Save the restart state
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single); // Load the main scene (replace "MainScene" with your main scene name)

    }
}
