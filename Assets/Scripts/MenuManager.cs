using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public GameObject pauseMenu;
    public GameObject optionsMenu;

    bool options;

    // Use this for initialization
    void Start ()
    {
        options = false;	
	}

    void Update()
    {
        bool setter = Globals.paused & !options;
        pauseMenu.SetActive(setter);
        setter = Globals.paused & options;
        optionsMenu.SetActive(setter);
    }

    public void setOptions(bool val)
    {
        options = val;
    }

}
