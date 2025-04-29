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
        if (instance != null && this.gameObject != null)
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "Menu" && instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
}

/*
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance { get { return instance; } }

    //public static int loadedScenes = 0;
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
        instance = (T)this;

        if (!gameObject.transform.parent)
        {
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
       // loadedScenes++;
        if (arg0.name == "Menu" && instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        // if(arg0.name == "Level 1" && instance != null)
        // {
        //     if(loadedScenes > 1)
        //     {
        //         Destroy(instance.gameObject);
        //         instance = null;
        //     }
        //     Destroy(instance.gameObject);
        //     instance = null;
        // }
    }
}
*/
