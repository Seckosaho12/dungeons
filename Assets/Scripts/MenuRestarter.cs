using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuRestarter : MonoBehaviour
{

    [SerializeField] private MainMenu mainMenu;

    private void Start()
    {
        if (PlayerPrefs.GetInt("Restarted") == 1)
        {
            PlayerPrefs.SetInt("Restarted", 0);
            mainMenu.PlayGame();
        }
    }
}
