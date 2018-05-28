using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{

    int AP;
    int AA;
    int maxAA;
    int ES;
    int maxES;
    int AF;
    int maxAF;

    int[] def = new int[3]; //AA, ES, AF.
    int numDefs; //how many defenses are still enabled.

    float tCount;

    int[] typeCount;

    public GameObject sprite;
    public Sprite[] faces;

    int activeDef; //0 = Armour, 1 = Shield, 2 = Flare

    public GameObject[] glowAnims;

    public Text textAP;
    public Slider sliderAA;
    public Text textAA;
    public Slider sliderES;
    public Text textES;
    public Slider sliderAF;
    public Text textAF;

    float timePassed;

    //public RuntimeAnimatorController rac;
    public GameObject splosion;
    bool sploding;
    bool flashing;
    bool matching;

    // Use this for initialization
    void Start()
    {
        AP = 10000;
        maxAA = 5000;
        def[0] = maxAA;
        maxES = 5000;
        def[1] = maxES;
        maxAF = 5000;
        def[2] = maxAF;
        activeDef = 0;
        numDefs = 3;

        tCount = 0;
        sploding = false;
        flashing = false;
        matching = false;
        timePassed = 0; 

        //UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(0);

        foreach(GameObject anim in glowAnims)
        {
            Debug.Log("blarg");
            anim.SetActive(false);
        }
        glowAnims[0].SetActive(true);

        typeCount = new int[3] { 0, 0, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.paused) return;
        timePassed += Time.deltaTime;

        sliderAA.value = def[0];
        sliderES.value = def[1];
        sliderAF.value = def[2];
        int APSum = def[0] + def[1] + def[2] + AP;
        textAP.text = APSum + ""; // + "AP";
        textAA.text = def[0] + "/" + maxAA;
        textES.text = def[1] + "/" + maxES;
        textAF.text = def[2] + "/" + maxAF;

        switch (activeDef)
        {
            case 0:
                //textAA.text += "\nACTIVE";
                break;
            case 1:
                //textES.text += "\nACTIVE";
                break;
            case 2:
                //textAF.text += "\nACTIVE";
                break;
            default:
                break;
        }
        if (def[0] <= 0) textAA.text = "DISABLED";
        if (def[1] <= 0) textES.text = "DISABLED";
        if (def[2] <= 0) textAF.text = "DISABLED";

        tCount += Time.deltaTime;
        if (tCount >= 5 && numDefs > 0)
        {
            updateDefenses();
        }
    }

    public void tCountIncrement(string type)
    {
        switch (type)
        {
            case "Ballistic":
                typeCount[0]++;
                break;
            case "Energy":
                typeCount[1]++;
                break;
            case "Explosive":
                typeCount[2]++;
                break;
        }

        Debug.Log(typeCount[1]);
    }

    public void playerProjHit(int damage, string type)
    {

        int t = -1;
        switch (type)
        {
            case "Ballistic":
                t = 0;
                break;
            case "Energy":
                t = 1;
                break;
            case "Explosive":
                t = 2;
                break;
        }
        matching = false;
        if (activeDef == t)
        {
            
            if (damage % 10 >= 5) damage += 5;
            damage /= 10;
            matching = true;
        }

        switch (activeDef)
        {
            case 0: //armour
                if (def[0] >= damage)
                    def[0] -= damage;
                else
                {
                    damage -= def[0]; def[0] = 0;
                    AP = Mathf.Clamp(AP - damage, 0, AP);
                    numDefs--;
                    updateDefenses();
                }
                break;
            case 1: //shield
                if (def[1] >= damage)
                    def[1] -= damage;
                else
                {
                    damage -= def[1]; def[1] = 0;
                    AP = Mathf.Clamp(AP - damage, 0, AP);
                    numDefs--;
                    updateDefenses();
                }
                break;
            case 2: //flare
                if (def[2] >= damage)
                    def[2] -= damage;
                else
                {
                    damage -= def[2]; def[2] = 0;
                    numDefs--;
                    AP = Mathf.Clamp(AP - damage, 0, AP);
                    updateDefenses();
                }
                break;
            default: //none left
                AP = Mathf.Clamp(AP - damage, 0, AP);
                break;
        }

        if (!flashing)
            StartCoroutine(hitFlash());
        if(!sploding)
            StartCoroutine(splode());
    }

    IEnumerator splode()    //plays explosion animation when enemy hit. 
    {
        sploding = true;
        splosion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        sploding = false;
        splosion.SetActive(false);
        yield return null;
    }

    IEnumerator hitFlash() //makes enemy flash when hit.
    {
        flashing = true;
        sprite.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
        yield return new WaitForSeconds(0.05f);
        sprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        yield return new WaitForSeconds(0.1f);
        flashing = false;
        yield return null;
    }

    public void spriteFacing(int i)
    {
        sprite.GetComponent<SpriteRenderer>().sprite = faces[i];
    }

    private void updateDefenses()
    {
        if (numDefs <= 0)
        {
            foreach (GameObject g in glowAnims)
            {
                g.SetActive(false);
            }
            tCount = -40;
            activeDef = 3;
            return;
        }

        if(timePassed > 20)
        {
            timePassed = 0;
            int test = 0;
            do
            {
                foreach (GameObject g in glowAnims)
                {
                    g.SetActive(false);
                }
                activeDef++;
                test++;
                if (activeDef >= 3) activeDef = 0;
                glowAnims[activeDef].SetActive(true);
            }
            while (def[activeDef] <= 0 && test < 10);
        }
        //Debug.Log("change defense mode");

        //int atkSum = typeCount[0] + typeCount[1] + typeCount[2];
        //Debug.Log(atkSum);
        //foreach (int i in typeCount)
        //{
        //    if (i >= 3) atkSum = 6;
        //}

        //if (atkSum >= 6 || def[activeDef] <= 0)
        //{
        //    //int test = 0;

        //    if (typeCount[0] > typeCount[1] && def[0] > 0)
        //    {
        //        if (typeCount[0] > typeCount[2])
        //            activeDef = 0;
        //        else if (typeCount[2] > 0 && def[2] > 0)
        //            activeDef = 2;
        //        else
        //            activeDef = 1;
        //    }
        //    else if (typeCount[1] > typeCount[2] && def[1] > 0)
        //        activeDef = 1;
        //    else if (def[2] > 0)
        //        activeDef = 2;
        //    else
        //        activeDef = 0;

        //do
        //{
        //    activeDef++;
        //    test++;
        //    if (activeDef >= 3) activeDef = 0;
        //} while (def[activeDef] <= 0 && test < 10);

        //    foreach(GameObject anim in glowAnims)
        //    {
        //        anim.SetActive(false);
        //    }
        //    glowAnims[activeDef].SetActive(true);

        //    typeCount[0] = typeCount[1] = typeCount[2] = 0;
        //    tCount = 0;
        //}
    }

    public void newAtk(int type)
    {
        typeCount[type]++;
    }

    public int getHealth()
    {
        return AP;
    }
}
