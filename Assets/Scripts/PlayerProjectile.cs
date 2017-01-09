using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class PlayerProjectile : MonoBehaviour {

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

    public GameObject flashLeft;
    public GameObject flashRight;

    public GameObject player;

    public Text weaponText;

    public GameObject[] panelButtons;
    public AudioClip[] weaponSounds;   //Flamethrower, Gauss, Ion, Javelin, LS, Railgun, RPG

    bool firing;
    bool[] isCooling;
    int[] coolTime;
    int equipped; //which weapon is equipped

    List<string> m_Weapons;

    float damageCounter;

    // Use this for initialization
    void Start ()
    {
        numShots = clip;
        damageCounter = 0;
        m_Weapons = new List<string>();
        loadWeapons();
        coolTime = new int[m_Weapons.Count];
        isCooling = new bool[m_Weapons.Count];
        equipped = -1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (firing)
        {
            Debug.Log(AP);
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
                Debug.Log("bloop");
                coolWeapon();
            }

            if (--numShots == 0)
            {
                reload();
            }
        }
        else if (!player.GetComponent<PlayerController>().getFiring())
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
        for (int i = 0; i<coolTime.Length; i++)
        {
            if(coolTime[i]>0) coolTime[i]--;
            else isCooling[i] = false;
        }
    }

    public void fire()
    {
        if (isCooling[equipped] == true) return;
        firing = true;
        fireTime = duration;
        player.GetComponent<PlayerController>().setFiring(true);
        flashLeft.SetActive(true);
        flashRight.SetActive(true);
        Debug.Log(soundEffect);
        gameObject.GetComponent<AudioSource>().PlayOneShot(weaponSounds[soundEffect],1.0f);


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
        player.GetComponent<PlayerController>().setFiring(false);
    }

    void reload()
    {
        ammo--;
        if(ammo > 0)
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

        foreach(GameObject o in panelButtons)
        {
            o.GetComponent<Image>().color = new Color(255, 255, 255, 100);
        }
        Color highlight = new Color();
        switch(type.ToLower())
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
            file = file + "/Resources/PWeapons.txt";
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
}
