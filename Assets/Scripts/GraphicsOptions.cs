using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsOptions : MonoBehaviour
{
    bool fullscreen;
    int resx;
    int resy;

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
                resx = 1920;
                resy = 1280;
                break;
            case 1:
                resx = 1280;
                resy = 720;
                break;
            case 2:
                resx = 720;
                resy = 526;
                break;
            case 3:
                resx = 640;
                resy = 480;
                break;
        }
        Screen.SetResolution(resx, resy, fullscreen);
    }

    public void setFullscreen(bool f)
    {
        fullscreen = f;
        Screen.SetResolution(resx, resy, fullscreen);
    }
}
