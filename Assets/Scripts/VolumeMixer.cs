using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeMixer : MonoBehaviour {

    public Text BGM;
    public Text SFX;
    public GameObject sourceBGM;
    public GameObject sourceSFX;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setSFXText(float val)
    {
        SFX.text = val.ToString();
        sourceSFX.GetComponent<AudioSource>().volume = (val * 0.01f);
    }

    public void setBGMText(float val)
    {
        BGM.text = val.ToString();
        sourceBGM.GetComponent<AudioSource>().volume = (val * 0.01f);
    }
}
