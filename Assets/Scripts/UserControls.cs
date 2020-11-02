using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControls : MonoBehaviour
{
    public static bool paused;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetPause(!paused);

        if (Input.GetKeyDown(KeyCode.F12))
            Screen.fullScreen = !Screen.fullScreen;

        if (paused)
            return;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    public static void SetPause(bool pause)
    {
        paused = pause;
        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
