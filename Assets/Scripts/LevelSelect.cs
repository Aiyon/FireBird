using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void levelSelect(int i)
    {
        Globals.setLevel("/L" + i);
        SceneManager.LoadScene(i);
    }

    public void qq()
    {
        Application.Quit();
    }
}
