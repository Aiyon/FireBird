using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class PlayerProjectile : MonoBehaviour
{

    public int AP;
    public int heat;    //heat generation
    public float minRange;
    public float maxRange;
    public int cooldown;
    int numShots;
    public int clip;    //shots per clip
    public int capacity;    //number of clips
    int ammo;               //clips remaining
    public int reloadTime;
    public int duration;    //how long it fires for, essentially shots per clip.
    float fireTime;
    public string type; //weapon type
    string name;
    int soundEffect;

    public string level; //LEVEL SETTER

    public GameObject flashLeft;
    public GameObject flashRight;

    public Text weaponText;

    public GameObject[] panelButtons;
    public AudioClip[] weaponSounds;   //Flamethrower, Gauss, Ion, Javelin, LS, Railgun, RPG

    bool firing;
    bool[] isCooling;
    int[] coolTime;
    int equipped; //which weapon is equipped

    List<string> m_Weapons;

    float damageCounter;

    //weapon range wheel tests.
    public Text gMinus3;
    public Text rMinus3;
    public Text gMinus2;
    public Text rMinus2;
    public Text gMinus1;
    public Text rMinus1;
    public Text g0;
    public Text gPlus1;
    public Text rPlus1;
    public Text gPlus2;
    public Text rPlus2;
    public Text gPlus3;
    public Text rPlus3;
    float prevPos;
    List<float> minRanges = new List<float>();
    List<float> maxRanges = new List<float>();
    List<string> names = new List<string>();
    List<int> sortedMinRange = new List<int>(); //List index rather than value.
    List<int> sortedMaxRange = new List<int>(); //List index rather than value.

    // Use this for initialization
    void Start()
    {
        numShots = clip;
        damageCounter = 0;
        m_Weapons = new List<string>();
        loadWeapons();
        rangeSort();
        coolTime = new int[m_Weapons.Count];
        isCooling = new bool[m_Weapons.Count];
        equipped = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (firing)
        {
            if (type.ToLower() == "explosive")   //ballistic, energy
            {
                //fire once for full AP
                hitEnemy(AP);
                fireTime = 0;
            }
            else
            {
                //fire for (AP/duration)*time.delta time, for duration.
                damageCounter += (AP / duration) * Time.deltaTime;
                fireTime -= Time.deltaTime;

                if ((damageCounter - damageCounter % 1) > 0)
                {
                    hitEnemy((int)(damageCounter - damageCounter % 1));
                    damageCounter = damageCounter % 1;
                }//If damage counter has exceeded at least one damage, deal damage rounded down to nearest int, then subtract said int from damage counter.
            }

            if (fireTime <= 0)
            {
                coolWeapon();
            }

            if (--numShots == 0)
            {
                reload();
            }
        }
        else if (!gameObject.GetComponent<PlayerController>().getFiring())
        {
            if (Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                if (equipped != 0) equip(0); else fire();
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (equipped != 1) equip(1); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                if (equipped != 2) equip(2); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                if (equipped != 3) equip(3); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                if (equipped != 4) equip(4); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                if (equipped != 5) equip(5); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                if (equipped != 6) equip(6); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                if (equipped != 7) equip(7); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                if (equipped != 8) equip(8); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                if (equipped != 9) equip(9); else fire();
            }
            else if (Input.GetKeyDown(KeyCode.KeypadDivide))
            {
                if (equipped != 10) equip(10); else fire();
            }
        }
        for (int i = 0; i < coolTime.Length; i++)
        {
            if (coolTime[i] > 0) coolTime[i]--;
            else isCooling[i] = false;
        }

        if (gameObject.transform.localPosition.z != prevPos) rangeUpdate();
    }

    public void fire()
    {
        if (isCooling[equipped] == true) return;

        float dist = gameObject.transform.localPosition.z * -1;
        if (minRange > dist || maxRange < dist)
        {
            Debug.Log("no range");
            return;
        }
        firing = true;
        fireTime = duration;
        gameObject.GetComponent<PlayerController>().setFiring(true);
        flashLeft.SetActive(true);
        flashRight.SetActive(true);
        Debug.Log(soundEffect);
        gameObject.GetComponent<AudioSource>().PlayOneShot(weaponSounds[soundEffect], 1.0f);


        int t = 0;
        switch (type.ToLower())
        {
            case "ballistic":
                t = 0;
                break;
            case "energy":
                t = 1;
                break;
            case "explosive":
                t = 2;
                break;
        }
        gameObject.GetComponent<PlayerController>().Enemy.GetComponent<EnemyHealth>().newAtk(t);

    }

    void coolWeapon()
    {
        isCooling[equipped] = true;
        coolTime[equipped] = cooldown;
        firing = false;
        flashLeft.SetActive(false);
        flashRight.SetActive(false);
        gameObject.GetComponent<PlayerController>().setFiring(false);
    }

    void reload()
    {
        ammo--;
        if (ammo > 0)
        { /* reload */ }
        else
        { /* out of ammo */ }
    }

    public void equip(int weapon)
    {
        equipped = weapon;

        string[] stats = m_Weapons[equipped].Split(',');
        //Name,Type,AP,heat,minRange,maxRange,Cooldown,reload,clip,capacity,AudioNumber
        name = stats[0];
        weaponText.text = name;
        type = stats[1];
        AP = int.Parse(stats[2]);
        minRange = float.Parse(stats[3]);
        maxRange = float.Parse(stats[4]);
        heat = int.Parse(stats[5]);
        cooldown = int.Parse(stats[6]);
        reloadTime = int.Parse(stats[7]);
        clip = int.Parse(stats[8]);
        capacity = int.Parse(stats[9]);
        soundEffect = int.Parse(stats[10]);

        foreach (GameObject o in panelButtons)
        {
            o.GetComponent<Image>().color = new Color(255, 255, 255, 100);
        }
        Color highlight = new Color();
        switch (type.ToLower())
        {
            case "explosive":
                highlight = new Color(255, 0, 0, 0.4f);
                break;

            case "ballistic":
                highlight = new Color(0, 0, 255, 0.4f);
                break;

            case "energy":
                highlight = new Color(0, 255, 0, 0.4f);
                break;

            default:
                highlight = new Color(255, 255, 0, 0.4f);
                break;
        }
        panelButtons[equipped].GetComponent<Image>().color = highlight;
        //get stats of equipped weapon (AP, heat, etc)
    }

    public bool getFiring()
    { return firing; }

    public bool outofAmmo()
    {
        return (ammo == 0);
    }

    public bool cooling()
    {
        return isCooling[equipped];
    }

    void hitEnemy(int damage)
    {
        gameObject.GetComponent<PlayerController>().Enemy.GetComponent<EnemyHealth>().playerProjHit(damage, type);
        Debug.Log("firing " + type + " for " + AP + " damage.");
    }

    private void loadWeapons()
    {
        try
        {
            string line;
            string file = Application.dataPath;
            file = file + "/Resources" + Globals.getLevel() + "/PWeapons.txt";
            StreamReader theReader = new StreamReader(file, Encoding.Default);

            using (theReader)
            {
                line = theReader.ReadLine();
                do
                {
                    line = theReader.ReadLine();
                    if (line != null)
                        m_Weapons.Add(line);
                }
                while (line != null);
                theReader.Close();
                return;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
    }

    void rangeUpdate()
    {

        //go through sorted ranges to find 3 weapons either side.
        float tempPos = gameObject.transform.localPosition.z * -1;

        for (int i = 0; i < sortedMaxRange.Count; i++)
        {

            if (maxRanges[sortedMaxRange[i]] > tempPos)
            {
                rMinus1.text = "";
                gMinus1.text = "";
                rMinus2.text = "";
                gMinus2.text = "";
                rMinus3.text = "";
                gMinus3.text = "";
                int t = i;
                t--;
                if (sortedMaxRange[t] == equipped) t--;
                if (t < 0) break;
                rMinus1.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gMinus1.text = names[sortedMaxRange[t]];
                t--;
                if (sortedMaxRange[t] == equipped) t--;
                if (t < 0) break;
                rMinus2.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gMinus2.text = names[sortedMaxRange[t]];
                t--;
                if (sortedMaxRange[t] == equipped) t--;
                if (t < 0) break;
                rMinus3.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gMinus3.text = names[sortedMaxRange[t]];

                break;
            }
        }
        for (int i = 0; i < sortedMaxRange.Count; i++)
        {
            if (maxRanges[sortedMaxRange[i]] > tempPos)
            {
                rPlus1.text = "";
                gPlus1.text = "";
                rPlus2.text = "";
                gPlus2.text = "";
                rPlus3.text = "";
                gPlus3.text = "";
                int t = i;
                if (sortedMaxRange[t] == equipped) t++;
                if (t >= maxRanges.Count) break;
                rPlus1.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gPlus1.text = names[sortedMaxRange[t]];
                t++;
                if (t >= maxRanges.Count) break;
                if (sortedMaxRange[t] == equipped) t++;
                if (t >= maxRanges.Count) break;
                rPlus2.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gPlus2.text = names[sortedMaxRange[t]];
                t++;
                if (t >= maxRanges.Count) break;
                if (sortedMaxRange[t] == equipped) t++;
                if (t >= maxRanges.Count) break;
                rPlus3.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gPlus3.text = names[sortedMaxRange[t]];
                break;
            }
        }

        if (maxRange < tempPos)
        {
            rMinus1.text = maxRange * 1000 + "m";
            gMinus1.text = name;
            g0.text = "";
        }
        else if (minRange > tempPos)
        {
            rPlus1.text = minRange * 1000 + "m";
            gPlus1.text = name;
            g0.text = "";
        }
        else
        {
            g0.text = name;
        }
        prevPos = tempPos;
    }

    void rangeSort()
    {
        List<float> tempMin = new List<float>();
        List<float> tempMax = new List<float>();
        List<float> tempMinSorted = new List<float>();
        List<float> tempMaxSorted = new List<float>();

        //set up sorted range lists
        for (int i = 0; i < m_Weapons.Count; i++)
        {
            tempMin.Add(float.Parse(m_Weapons[i].Split(',')[3]));
            tempMax.Add(float.Parse(m_Weapons[i].Split(',')[4]));
            names.Add(m_Weapons[i].Split(',')[0]);

            sortedMinRange.Add(0);
            sortedMaxRange.Add(0);
        }

        minRanges = new List<float>(tempMin); tempMinSorted = new List<float>(tempMin);
        tempMinSorted.Sort();
        maxRanges = new List<float>(tempMax); tempMaxSorted = new List<float>(tempMax);
        tempMaxSorted.Sort();

        //for each in sorted, go through temp and find one that matches, put id into sMR index then set match as -1.
        for (int i = 0; i < tempMaxSorted.Count; i++)
        {
            for (int j = 0; j < tempMax.Count; j++)
            {
                if (tempMaxSorted[i] == tempMax[j])
                {
                    sortedMaxRange[i] = j;
                    tempMax[j] = -1;
                    break;
                }
            }
        }
        for (int i = 0; i < tempMinSorted.Count; i++)
        {
            for (int j = 0; j < tempMin.Count; j++)
            {
                if (tempMinSorted[i] == tempMin[j])
                {
                    sortedMinRange[i] = j;
                    tempMin[j] = -1;
                    break;
                }
            }
        }
    }
}
