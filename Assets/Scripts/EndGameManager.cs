using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private GameEndArea gameEndArea; // Reference to the GameEndArea script
    [SerializeField] private Button restartButton; // Reference to the restart button
    [SerializeField] private Button quitToMenuButton; // Reference to the quit to menu button
    [SerializeField] private Button quitToDesktopButton; // Reference to the quit to desktop button
    [SerializeField] private GameObject gameEndUI; // Reference to the game end UI

    void Start()
    {
        gameEndArea.OnGameEnd += ShowGameEndUI; // Subscribe to the OnGameEnd event
        // Ensure the game end UI is hidden at the start
        if (gameEndUI != null)
        {
            gameEndUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Game End UI is not assigned in the Inspector.");
        }

        // Add listeners to buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(delegate { PauseMenuManager.Instance.RestartGame(); });
        }
        else
        {
            Debug.LogError("Restart Button is not assigned in the Inspector.");
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.AddListener(delegate { PauseMenuManager.Instance.QuitToMainMenu(); });
        }
        else
        {
            Debug.LogError("Quit to Menu Button is not assigned in the Inspector.");
        }

        if (quitToDesktopButton != null)
        {
            quitToDesktopButton.onClick.AddListener(delegate { PauseMenuManager.Instance.QuitToDesktop(); });
        }
        else
        {
            Debug.LogError("Quit to Desktop Button is not assigned in the Inspector.");
        }
    }

    private void ShowGameEndUI()
    {
        if (gameEndUI != null)
        {
            gameEndUI.SetActive(true); // Show the game end UI
        }
        else
        {
            Debug.LogError("Game End UI is not assigned in the Inspector.");
        }
    }
}
