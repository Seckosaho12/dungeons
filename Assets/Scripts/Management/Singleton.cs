using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance { get { return instance; } }
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = (T)this;
        }
        if (!gameObject.transform.parent)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += DestroyIfMenuSceneLoaded;
    }

    private void DestroyIfMenuSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "Menu" && instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
}
