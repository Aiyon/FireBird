using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsOptions : MonoBehaviour
{
    bool fullscreen;

    public void start()
    {
        fullscreen = false;
        //pull fullscreen from saved file.
    }

	public void setResolution(int v)
    {
        switch(v)
        {
            case 0:
                Screen.SetResolution(1920, 1280, fullscreen);
                break;
            case 1:
                Screen.SetResolution(1280, 720, fullscreen);
                break;
            case 2:
                Screen.SetResolution(720, 526, fullscreen);
                break;
            case 3:
                Screen.SetResolution(640, 480, fullscreen);
                break;
        }
    }

    public void setFullscreen(bool f)
    {
        fullscreen = f;
    }
}
