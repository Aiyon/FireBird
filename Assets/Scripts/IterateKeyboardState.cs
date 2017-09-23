using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IterateKeyboardState : MonoBehaviour
{
    public GameObject[] keyboards;
    public GameObject player;

    public void buttonHit(int i)
    {
        
        Globals.setKeySetting(i);
        //gameObject.GetComponentInChildren<Text>().text = "state:" + Globals.getKeySetting();
        player.GetComponent<PlayerProjectile>().setKeyboard(i);

        //for(int j = 0; j < keyboards.Length; j++)
        //{
        //    if (j != i)
        //    {
        //        keyboards[j].SetActive(false);
        //    }
        //    else keyboards[j].SetActive(true);
        //}
    }
}
