using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Globals
{

    static string Level = "/L1";

    public static string getLevel()
    { return Level; }
    public static void setLevel(string s)
    { Level = s; }

    //public static void levelSelect(int i)
    //{
    //    SceneManager.LoadScene(i);
    //}

    public static bool paused;

    private static int keySetting;

    public static void setKeySetting(int i)
    {
        keySetting = i;
    }
    
    public static int getKeySetting()
    { return keySetting; }
}
