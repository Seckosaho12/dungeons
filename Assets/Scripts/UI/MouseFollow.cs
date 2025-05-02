using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    private void Update()
    {
        if (PauseMenuManager.Instance.IsPaused()) return;

        FaceMouse();
    }

    private void FaceMouse()
    {
        if (VirtualCursor.Instance.isUsingController)
        {
            transform.right = VirtualCursor.Instance.GetLookVector();
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition + new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector2 direction = mousePosition.normalized;

            transform.right = -direction;
        }
    }
}

