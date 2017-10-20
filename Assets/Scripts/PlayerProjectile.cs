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
    public float heat;    //heat generation
    public float minRange;
    public float maxRange;
    public int cooldown;

    public int clip;    //shots per clip
    public int capacity;    //number of clips
    int[] ammo;               //ammo remaining in each weapon's clip
    int[] capacities;

    public int reload;
    public int duration;    //how long it fires for, essentially shots per clip.
    float fireTime;
    public string type; //weapon type
    string wName;
    int soundEffect;
    bool eFire; //so explosive only hits once.

    //range rings
    public GameObject inRange;
    public GameObject tooClose;

    //heat
    public Slider heatSlider;
    public Image sliderbar;
    bool overheated;

    //audio times for anim syncup
    public float[] audioStart;
    public float[] audioEnd;
    //animation speeds
    float animSpeed;

    public string level; //LEVEL SETTER

    public GameObject flashLeft;
    public GameObject flashRight;
    public RuntimeAnimatorController[] animMain;
    public RuntimeAnimatorController animBulletR;

    public Text weaponText;

    GameObject[] panelButtonsCurrent;
    public GameObject[] panelButtons0;
    public GameObject[] panelButtons1;
    public GameObject[] panelButtons2;
    public GameObject[] keyboardSets;
    public Sprite[] buttonTypes;
    public AudioClip[] weaponSounds;   //Flamethrower, Gauss, Ion, Javelin, LS, Railgun, RPG
    
    bool firing;
    bool[] isCooling;
    int[] coolTime;
    int[] maxCoolTime;
    bool[] inOperable;

    bool[] isReloading;
    int[] reloadTime;
    int[] maxReloadTime;

    //portal stuff
    bool[] portalCool;
    int[] portalCoolTime;
    float portalHeat;
    bool portalEntry = false;
    bool portalExit = false;
    public GameObject portalIn;
    public GameObject portalOut;
    float portalCD = 0;
    bool ported = false;

    int equipped; //which weapon is equipped
    bool atkDuration; //used to manually enable/disable damage to allow syncing to animation/audio

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
        damageCounter = 0;
        m_Weapons = new List<string>();
        loadWeapons();
        rangeSort();
        coolTime = new int[m_Weapons.Count];
        isCooling = new bool[m_Weapons.Count];

        reloadTime = new int[m_Weapons.Count];
        isReloading = new bool[m_Weapons.Count];
        inOperable = new bool[m_Weapons.Count];
        
        maxCoolTime = new int[m_Weapons.Count];
        maxReloadTime = new int[m_Weapons.Count];

        animSpeed = 0.0f;

        panelButtonsCurrent = new GameObject[17];
        Globals.setKeySetting(0);
        setKeyboard(Globals.getKeySetting());

        for (int i = 0; i < m_Weapons.Count; i++)
        {
            maxCoolTime[i] = int.Parse(m_Weapons[i].Split(',')[6])*60; //cool
            maxReloadTime[i] = int.Parse(m_Weapons[i].Split(',')[7])*60; //reload
        }

        inRange.transform.localScale = Vector3.zero;
        tooClose.transform.localScale = Vector3.zero;

        overheated = false;
        heatSlider.value = 0;
        ammo = new int[m_Weapons.Count];
        capacities = new int[m_Weapons.Count];
        for (int i = 0; i < ammo.Length; i++)
        {
            ammo[i] = -1;
            capacities[i] = -1;
        }

        portalCool = new bool[isCooling.Length];
        portalCoolTime = new int[coolTime.Length];

        equip(5);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<PlayerController>().getFiring() && !firing)
        {
            switch(Globals.getKeySetting())
            {
                case 0:
                    keyboardCheck0();
                    break;
                case 1:
                    keyboardCheck1();
                    Debug.Log("Blarg");
                    break;
                case 2:
                    keyboardCheck2();
                    break;
            }
        }


        if (Globals.paused) return;

        if (firing)
        {
            if (type.ToLower() == "explosive")   //ballistic, energy
            {
                //fire once for full AP
                fireTime -= Time.deltaTime;

                if (eFire)
                {
                    hitEnemy(AP);
                    heatSlider.value += heat;
                    eFire = false;
                }
            }
            else
            {
                if (atkDuration)
                {
                    float atkDur = audioEnd[equipped] - audioStart[equipped];
                    //fire for (AP/duration)*time.delta time, for duration.
                    damageCounter += (AP / atkDur) * Time.deltaTime;
                    heatSlider.value += heat * Time.deltaTime * (duration/atkDur);
                }
            
                fireTime -= Time.deltaTime;

                if ((damageCounter - damageCounter % 1) > 0)
                {
                    hitEnemy((int)(damageCounter - damageCounter % 1));
                    damageCounter = damageCounter % 1;
                }//If damage counter has exceeded at least one damage, deal damage rounded down to nearest int, then subtract said int from damage counter.
            }

            if (heatSlider.value >= 100) overheated = true;

            if (fireTime <= 0)
            {
                ammo[equipped]--;
                //Debug.Log("Ammo: " + ammo[equipped]);
                if (ammo[equipped] == 0)
                {
                    m_fReload();//reload
                }
                else
                {
                    coolWeapon();
                }
            }
        }

        //Heat slider colour update.
        if (!overheated)
        {
            float colour = (153 - heatSlider.value)/255;
            sliderbar.color = new Color(1, colour, 0, 1);
        }
        else
            sliderbar.color = new Color(1, 0.21f, 0, 1);

        //COOLDOWN/RELOAD UI TIMERS
        Color lightSetter;
        for (int i = 0; i < coolTime.Length; i++)
        {
            float angle = 0;
            lightSetter = new Color(60, 60, 60, 0);
            if (isCooling[i])
            {
                if (coolTime[i] > 0)
                {
                    coolTime[i]--;
                    lightSetter.a = 0.75f;
                    float c1 = coolTime[i]; float c2 = maxCoolTime[i];
                    angle = c1 / c2;
                    //Debug.Log(c1 + ", " + c2 + ", " + angle);
                    panelButtonsCurrent[i].transform.GetChild(0).GetComponent<Image>().fillAmount = angle;
                }
                else
                {
                    lightSetter.a = 0.75f;
                    float c1 = coolTime[i]; float c2 = maxCoolTime[i];
                    angle = c1 / c2;
                    isCooling[i] = false;
                }
            }
            else if (isReloading[i] && reloadTime[i] > 0)
            {
                reloadTime[i]--;
                lightSetter.a = 0.75f;
                float r1 = reloadTime[i]; float r2 = maxReloadTime[i];
                angle = r1/r2; 
            }
            else
            {
                lightSetter.a = 0.75f;
                float r1 = reloadTime[i]; float r2 = maxReloadTime[i];
                angle = r1 / r2;
                isReloading[i] = false;
            }

            panelButtonsCurrent[i].transform.GetChild(0).GetComponent<Image>().color = lightSetter;
            panelButtonsCurrent[i].transform.GetChild(0).GetComponent<Image>().fillAmount = angle;
            //maxCoolTime / maxReloadTime;
        }

        heatSlider.value -= Time.deltaTime;
        if (overheated)
        {
            if (heatSlider.value < 70) overheated = false;
            else heatSlider.value -= Time.deltaTime;
        }
        if (gameObject.transform.localPosition.z != prevPos) rangeUpdate();

        if (portalEntry)
            portalIn.transform.LookAt(Camera.main.transform.position, -Vector3.up);
        if (portalExit)
            portalOut.transform.LookAt(Camera.main.transform.position, -Vector3.up);

        if(ported)
        {
            portalCD -= Time.deltaTime;
            if(portalCD <= 0)
            {
                portalIn.GetComponent<SpriteRenderer>().enabled = true;
                portalOut.GetComponent<SpriteRenderer>().enabled = true;
                ported = false;
            }
        }
        else if (portalEntry && portalExit)
        {
            Vector3 portalDist = gameObject.transform.position - portalIn.transform.position;
            if(portalDist.magnitude <= 2)
            {
                gameObject.transform.position = portalOut.transform.position;
                Array.Copy(portalCool, isCooling, portalCool.Length);
                Array.Copy(portalCoolTime, coolTime, portalCoolTime.Length);
                heatSlider.value = portalHeat;
                portalCD = 120;
                ported = true;
                portalIn.GetComponent<SpriteRenderer>().enabled = false;
                portalOut.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        else
        {
            if (!portalEntry && placePortalEntrance())
            {
                //place portal entry at player.
                if(portalExit)
                {
                    //check distance, if <5, dont place
                }
                portalIn.transform.position = gameObject.transform.position;
                portalIn.GetComponent<SpriteRenderer>().enabled = true;
                panelButtonsCurrent[12].GetComponent<Image>().color = new Color(0.5f,0.5f,0.5f, 0.3f);
                portalEntry = true;
            }
            if (!portalExit && placePortalExit())
            {
                //place portal exit at player
                portalOut.transform.position = gameObject.transform.position;
                portalOut.GetComponent<SpriteRenderer>().enabled = true;
                Array.Copy(isCooling, portalCool, isCooling.Length);
                Array.Copy(coolTime, portalCoolTime, coolTime.Length);
                portalHeat = heatSlider.value;
                Debug.Log("PH: " + portalHeat);
                panelButtonsCurrent[16].GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                portalExit = true;
            }
        }
    }

    public void fire()
    {
        if (Globals.paused)
            return;

        if (overheated)
        {
            return;
        }
        if (inOperable[equipped] == true)
            return;

        if (isCooling[equipped] == true)
        {
            Debug.Log("Cooling.");
            return;
        }
        if (isReloading[equipped] == true)
        {
            Debug.Log("Reloading.");
            return;
        }

        float dist = gameObject.transform.localPosition.z * -1;
        if (minRange > dist || maxRange < dist)
        {
            Debug.Log("no range");
            return;
        }
        firing = true;
        fireTime = duration;
        gameObject.GetComponent<PlayerController>().setFiring(true);
        
        
        //flashLeft.SetActive(true);
        
        //flashRight.SetActive(true);
        

        if (soundEffect != -1) gameObject.GetComponent<AudioSource>().PlayOneShot(weaponSounds[soundEffect], 1.0f);


        int t = 0;
        switch (type.ToLower())
        {
            case "ballistic":
                StartCoroutine(animToggle(equipped, 0));
                //flashLeft.GetComponent<Animator>().runtimeAnimatorController = animMain[0];
                //flashRight.GetComponent<Animator>().runtimeAnimatorController = animBulletR;
                //flashRight.SetActive(true);
                t = 0;
                break;
            case "energy":
                StartCoroutine(animToggle(equipped, 1));
                //flashLeft.GetComponent<Animator>().runtimeAnimatorController = animMain[1];
                t = 1;
                break;
            case "explosive":
                StartCoroutine(animToggle(equipped, 2));
                //flashLeft.GetComponent<Animator>().runtimeAnimatorController = animMain[2];
                //eFire = true;
                t = 2;
                break;
        }
        //flashLeft.SetActive(true);

        gameObject.GetComponent<PlayerController>().Enemy.GetComponent<EnemyHealth>().newAtk(t);

    }

    IEnumerator animToggle(int e, int t)
    {

        Animator anim = flashLeft.GetComponent<Animator>();
        Animator anim2 = flashRight.GetComponent<Animator>();
        yield return new WaitForSeconds(audioStart[e]);
        atkDuration = true;
        switch (t)
        {
            case 0:
                anim.runtimeAnimatorController = animMain[0];
                anim2.GetComponent<Animator>().runtimeAnimatorController = animBulletR;
                flashRight.SetActive(true);
                break;
            case 1:
                anim.runtimeAnimatorController = animMain[1];
                break;
            case 2:
                anim.runtimeAnimatorController = animMain[2];
                eFire = true;
                break;
        }

        
        anim.speed = 1;
        anim2.speed = 1;
        switch (e)
        {
            case 7:
                anim.speed = anim2.speed = 3;
                break;
            default:
                break;
        }

        flashLeft.SetActive(true);
        float s = audioEnd[e] - audioStart[e];
        yield return new WaitForSeconds(s);
        atkDuration = false;

        flashLeft.SetActive(false);
        flashRight.SetActive(false);

        yield return null;
    }

    IEnumerator damageToggle(float start, float end)
    {
        yield return new WaitForSeconds(start);
        atkDuration = true;
        yield return new WaitForSeconds(end-start);
        atkDuration = false;
        yield return null;
    }

    void coolWeapon()
    {
        isCooling[equipped] = true;
        coolTime[equipped] = cooldown;
        firing = false;
        //flashLeft.SetActive(false);
        //flashRight.SetActive(false);
        gameObject.GetComponent<PlayerController>().setFiring(false);
    }

    void m_fReload()
    {
        capacities[equipped]--;
        if (capacities[equipped] > 0)
        {
            isReloading[equipped] = true;
            reloadTime[equipped] = reload;
            ammo[equipped] = clip;
            /* reload */
        }
        else
        {
            inOperable[equipped] = true;
            /* out of ammo */
        }
    }

    public void equip(int weapon)
    {
        switch(type.ToLower())
        {
            case "explosive":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[2];
                break;

            case "ballistic":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[3];
                break;

            case "energy":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[1];
                break;

            default:
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[4];
                break;
        }
        equipped = weapon;

        string[] stats = m_Weapons[equipped].Split(',');
        //Name,Type,AP,heat,minRange,maxRange,Cooldown,reload,clip,capacity,AudioNumber
        wName = stats[0];
        weaponText.text = wName;
        type = stats[1];
        AP = int.Parse(stats[2]);
        minRange = float.Parse(stats[3]);
        maxRange = float.Parse(stats[4]);
        heat = float.Parse(stats[5]);
        cooldown = int.Parse(stats[6]) * 60; //file = secs, not frames
        reload = int.Parse(stats[7]) *60;   //as above
        clip = int.Parse(stats[8]);
        if (ammo[equipped] == -1)
            ammo[equipped] = clip;
        capacity = int.Parse(stats[9]);
        if (capacities[equipped] == -1)
            capacities[equipped] = capacity;
        soundEffect = int.Parse(stats[10]);
        switch (type.ToLower())
        {
            case "explosive":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[7];
                break;

            case "ballistic":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[8];
                break;

            case "energy":
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[6];
                break;

            default:
                panelButtonsCurrent[equipped].GetComponent<Image>().sprite = buttonTypes[10];
                break;
        }
        animSpeed = float.Parse(stats[11]);

        //get stats of equipped weapon (AP, heat, etc)

        inRange.transform.localScale = new Vector3(maxRange*0.4f,maxRange*0.4f,1.0f);
        tooClose.transform.localScale = new Vector3(minRange*0.4f, minRange*0.4f, 1.0f);
    }

    public bool getFiring()
    { return firing; }

    public bool outofAmmo()
    {
        return (clip == 0);
    }

    public bool cooling()
    {
        return isCooling[equipped];
    }

    void hitEnemy(int damage)
    {
        gameObject.GetComponent<PlayerController>().Enemy.GetComponent<EnemyHealth>().playerProjHit(damage, type);
        //Debug.Log("firing " + type + " for " + AP + " damage.");
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
                if (t < 0) break;
                if (sortedMaxRange[t] == equipped) t--;
                if (t < 0) break;
                rMinus1.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gMinus1.text = names[sortedMaxRange[t]];
                t--;
                if (t < 0) break;
                if (sortedMaxRange[t] == equipped) t--;
                if (t < 0) break;
                rMinus2.text = maxRanges[sortedMaxRange[t]] * 1000 + "m";
                gMinus2.text = names[sortedMaxRange[t]];
                t--;
                if (t < 0) break;
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
            gMinus1.text = wName;
            g0.text = "";
        }
        else if (minRange > tempPos)
        {
            rPlus1.text = minRange * 1000 + "m";
            gPlus1.text = wName;
            g0.text = "";
        }
        else
        {
            g0.text = wName;
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

    public void setKeyboard(int i)
    {
        for(int j = 0; j<3; j++)
        {
            if (j == i) keyboardSets[j].SetActive(true);
            else keyboardSets[j].SetActive(false);
        }
        switch(i)
        {
            case 0:
                panelButtonsCurrent = panelButtons0;
                break;
            case 1:
                panelButtonsCurrent = panelButtons1;
                break;
            case 2:
                panelButtonsCurrent = panelButtons2;
                break;
            default:
                break;
        }
        Globals.setKeySetting(i);
    }

    void keyboardCheck0()
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

    void keyboardCheck1()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            if (equipped != 0) equip(0); else fire();
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            if (equipped != 1) equip(1); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            if (equipped != 2) equip(2); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.Slash))
        {
            if (equipped != 3) equip(3); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (equipped != 4) equip(4); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            if (equipped != 5) equip(5); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (equipped != 6) equip(6); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            if (equipped != 7) equip(7); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            if (equipped != 8) equip(8); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (equipped != 9) equip(9); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (equipped != 10) equip(10); else fire();
        }
    }

    void keyboardCheck2()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (equipped != 0) equip(0); else fire();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (equipped != 1) equip(1); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (equipped != 2) equip(2); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            if (equipped != 3) equip(3); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (equipped != 4) equip(4); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (equipped != 5) equip(5); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (equipped != 6) equip(6); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (equipped != 7) equip(7); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (equipped != 8) equip(8); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (equipped != 9) equip(9); else fire();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (equipped != 10) equip(10); else fire();
        }
    }

    bool placePortalEntrance()
    {
        switch (Globals.getKeySetting())
        {
            case 0:
                return Input.GetKeyDown(KeyCode.KeypadPlus);
            case 1:
                return Input.GetKeyDown(KeyCode.RightBracket);
            case 2:
                return Input.GetKeyDown(KeyCode.Alpha5);

            default:
                return false;
        }
    }

    bool placePortalExit()
    {
        switch (Globals.getKeySetting())
        {
            case 0:
                return Input.GetKeyDown(KeyCode.KeypadEnter);
            case 1:
                return Input.GetKeyDown(KeyCode.Quote);
            case 2:
                return Input.GetKeyDown(KeyCode.T);

            default:
                return false;
        }
    }
}
