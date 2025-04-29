using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndArea : MonoBehaviour
{
    public Action OnGameEnd; // Declare the event
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnGameEnd?.Invoke(); // Invoke the event when the player enters the trigger
            PauseMenuManager.Instance.EndGame();
        }
    }
}
