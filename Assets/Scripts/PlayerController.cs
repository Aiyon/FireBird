using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	private Vector3 pRadius;
	private GameObject parent;
	public GameObject Enemy;

    public GameObject cam;
    private float shake = 0;
    private float fShake = 0;
    private float shakeAmt = 0.14f;
    private float decrement = 0.4f;
    private float fireShake = 0.05f;

    public GameObject bgClouds;
    public GameObject bgMountain;

    public int rotSpeed;
	private float rMomentum;
    private float rAccel;
	public float radialSpeed;
	private float zMomentum;
    private float zAccel;
	private float adjSpeed;

	public int minDist;
	public int maxDist;

	private float currentHealth;
	public int maxHealth;
	//public Slider healthSlider;
	public Text healthText;

    public Text rangeText;

    bool[] DashCheck = new bool[4];
    float dashTimer;
    float dashCool; //cooldown time on dash
    bool dashing = false;
    int dashID;
    float dashDuration;

    public RuntimeAnimatorController blankAnim;

//    int equipped = 0;
    bool firing;

    bool outpacingL;    //can the enemy turn fast enough to keep up with the player?
    bool outpacingR;
    float OPTimer;

    //enemy tracking
    List<float> pMomentum;
    float checkInterval;

    public Sprite[] facings;
    public RuntimeAnimatorController [] anims;
    public RuntimeAnimatorController[] shootAnims;

    //0 = idle, 1 = left, 2 = right, 3 = forward-left, 4 = forward-right, 5 = back-left, 6 = back-right, 7 = forward, 8 = back
    public GameObject sprite;

    bool playerHit;
    float pHAngle;
    float pHDist;
    

    int oldrangeSet;
    int newrangeSet;

    public GameObject[] shortRings;
    public GameObject[] medRings;
    public GameObject[] longRings;
    public GameObject[] xlRings;

    //Endgame UI screens
    public GameObject winScreen;
    public GameObject loseScreen;
    
    // Use this for initialization
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        currentHealth = maxHealth;
        rMomentum = 0;
        outpacingL = outpacingR = false;
        OPTimer = 0;
        loadPlayerStats();
        dashDuration = 0;
        dashID = -1;
        checkInterval = 20;
        pMomentum = new List<float>();
        playerHit = false;

        oldrangeSet = 5;
        newrangeSet = 3; 

        Globals.paused = false;
        Globals.gameState = 0;
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.gameState != 0) return;
        else
        {
            if (Enemy.GetComponent<EnemyHealth>().getHealth() <= 0)
            {
                Globals.gameState = 1;
                winScreen.SetActive(true);
                gameObject.GetComponent<PlayerProjectile>().setGameState(1);
            }
            if (currentHealth <= 0)
            {
                Globals.gameState = 2;
                loseScreen.SetActive(true);
                gameObject.GetComponent<PlayerProjectile>().setGameState(2);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Globals.paused = !Globals.paused;
            GameObject[] objs = GameObject.FindGameObjectsWithTag("projectile");
            foreach (GameObject g in objs)
            {
                g.transform.GetChild(0).gameObject.SetActive(!Globals.paused);
            }
        }
        if (Globals.paused)
            return;


        float r = gameObject.transform.localPosition.z * -1;
        if (r <= 15) newrangeSet = 0;
        else if (r <= 30) newrangeSet = 1;
        else if (r <= 45) newrangeSet = 2;
        else newrangeSet = 3;

        if(newrangeSet != oldrangeSet)
        {
            bool g0 = false;
            bool g1 = false;
            bool g2 = false;
            bool g3 = false;

            switch (newrangeSet)
            {
                case 0:
                    g0 = true;
                    break;
                case 1:
                    g1 = true;
                    break;
                case 2:
                    g2 = true;
                    break;
                case 3:
                    g3 = true;
                    break;
            }

            foreach (GameObject g in shortRings)
            { g.SetActive(g0); }
            foreach (GameObject g in medRings)
            { g.SetActive(g1); }
            foreach (GameObject g in longRings)
            { g.SetActive(g2); }
            foreach (GameObject g in xlRings)
            { g.SetActive(g3); }


            oldrangeSet = newrangeSet;
        }

        if (playerHit)
        {
            //if (pHAngle < 0)
            //{
            //    pHAngle += 2;
            //    if (pHAngle > 0) pHAngle = 0;
            //}
            //if(pHDist > 0)
            //{
            //    pHDist -= 4 * Time.deltaTime;
            //    if (pHDist < 0) pHDist = 0;
            //}

            //if (pHAngle == 0 && pHDist == 0) playerHit = false;
        }


        bool strafe = false;

        //healthSlider.value = currentHealth * healthSlider.maxValue / maxHealth;

        healthText.text = currentHealth + "";
        //---

        adjSpeed = rotSpeed / pRadius.z; //adjusted for player distance
        if (adjSpeed < 0) adjSpeed *= -1;

        float tSpeed = 0;

        if (!moveKeyCheck("W") && !moveKeyCheck("S"))
        {
            zMomentum = AccelerateTowards(zMomentum, 0, radialSpeed / 30);
        }

        if (!moveKeyCheck("A") && !moveKeyCheck("D"))
        {
            rMomentum = AccelerateTowards(rMomentum, 0, adjSpeed * rAccel * 0.7f);
        }
        if (!dashing) //cant change direction while dashing.
        {
            //CHECK IF PLAYER IS MOVING IN Z BEFORE DOING POLAR MOVE
            if (moveKeyCheck("W") || moveKeyCheck("S"))
                adjSpeed /= Mathf.Sqrt(2);

            //POLAR/ROTATION MOVE
            if (moveKeyCheck("A") && !moveKeyCheck("D"))
            {
                strafe = true;
                rMomentum = AccelerateTowards(rMomentum, adjSpeed, adjSpeed * rAccel);
            }
            else if (moveKeyCheck("D"))
            {
                rMomentum = AccelerateTowards(rMomentum, adjSpeed * -1, adjSpeed * rAccel);
            }


            //RADIUS MOVE
            tSpeed = radialSpeed;
            if (strafe) tSpeed /= Mathf.Sqrt(2);

            if (moveKeyCheck("W") && !moveKeyCheck("S"))
            {
                zMomentum = AccelerateTowards(zMomentum, tSpeed, tSpeed / 10);
            }
            else if (moveKeyCheck("S"))
            {
                zMomentum = AccelerateTowards(zMomentum, tSpeed * -1, tSpeed / 10);
            }
        }

        // MOMENTUM
        if (zMomentum != 0)
        {
            pRadius.z += zMomentum * Time.deltaTime;
            pRadius.z = Mathf.Clamp(pRadius.z, -maxDist, -minDist);
            if (zMomentum > tSpeed) zMomentum -= tSpeed / 10;
        }
        if (pRadius != gameObject.transform.localPosition)
        {
            gameObject.transform.localPosition = pRadius;
        }
        if (rMomentum != 0)
        {
            strafe = true;
            parent.transform.Rotate(0, rMomentum * Time.deltaTime, 0);
            Enemy.transform.Rotate(0, rMomentum * Time.deltaTime, 0);

            Vector3 bgT = bgMountain.transform.localPosition;
            bgT.x += rMomentum * Time.deltaTime * 0.005f;
            if (bgT.x >= 1.33f) bgT.x -= 1.33f;
            if (bgT.x <= -1.33f) bgT.x += 1.33f;
            bgMountain.transform.localPosition = bgT;

            bgT = bgClouds.transform.localPosition;
            bgT.x += rMomentum * Time.deltaTime * 0.004f;
            if (bgT.x >= 1.33f) bgT.x -= 1.33f;
            if (bgT.x <= -1.33f) bgT.x += 1.33f;
            bgClouds.transform.localPosition = bgT;

            if (Mathf.Abs(rMomentum) >= adjSpeed * 0.9f)
            {
                if (rMomentum > 0 && !outpacingR)
                {
                    Enemy.GetComponent<EnemyHealth>().spriteFacing(2);
                    outpacingL = false; outpacingR = true;
                    OPTimer = 1.0f;
                }
                else if (rMomentum < 0 && !outpacingL)
                {
                    Enemy.GetComponent<EnemyHealth>().spriteFacing(1);
                    outpacingL = true;
                    OPTimer = 1.0f;
                }
            }
            else if (Mathf.Abs(rMomentum) < adjSpeed * 0.1f)
                if (outpacingL || outpacingR)
                {
                    outpacingL = outpacingR = false;
                    Enemy.GetComponent<EnemyHealth>().spriteFacing(0);
                }
        }

        //dashing (0-3 = WASD)
        dashTimer -= Time.deltaTime;
        dashDuration -= Time.deltaTime;

        if (dashDuration > 0)
        {
            switch (dashID)
            {
                case 0:
                    zMomentum = radialSpeed * 2f;
                    break;
                case 1:
                    rMomentum = adjSpeed * 1.5f;
                    break;
                case 2:
                    zMomentum = radialSpeed * -2f;
                    break;
                case 3:
                    rMomentum = adjSpeed * -1.5f;
                    break;
                default:
                    Debug.Log("didn't get an ID");
                    break;
            }
        }
        else
        {
            if (dashing)
            {
                dashCool = 1.0f;
                dashing = false;
            }
            else
            {
                if (dashCool > 0) dashCool -= Time.deltaTime;
                else
                {
                    if (DashChecker(KeyCode.W, 0))
                        dashID = 0;
                    if (DashChecker(KeyCode.A, 1))
                        dashID = 1;
                    if (DashChecker(KeyCode.S, 2))
                        dashID = 2;
                    if (DashChecker(KeyCode.D, 3))
                        dashID = 3;
                }
            }
        }

        //set non-idle facings.
        if (rMomentum == 0)
        {
            if (zMomentum == 0)
                setFacing(0);
            else if (zMomentum <= 0)
                setFacing(8);
            else setFacing(7);
        }
        else if (rMomentum > 0)  //clockwise aka left.
        {
            if (zMomentum == 0)
                setFacing(1);
            else if (zMomentum > 0)
                setFacing(3);
            else setFacing(5);
        }
        else
        {
            if (zMomentum == 0)
                setFacing(2);
            else if (zMomentum > 0)
                setFacing(4);
            else
                setFacing(6);
        }

        if (OPTimer > 0 && (outpacingR || outpacingL))
        {
            OPTimer -= Time.deltaTime;
            if (OPTimer <= 0)
                Enemy.GetComponent<EnemyHealth>().spriteFacing(0);
        }

        Vector3 camPos = cam.transform.localPosition;
        camPos.x = rMomentum / rotSpeed;
        cam.transform.localPosition = camPos;

        float tFloat = gameObject.transform.localPosition.z * -1000;
        int trueRange = (int)tFloat;
        rangeText.text = trueRange + "m";

        if (!dashing)
        {
            checkInterval++;
            if (checkInterval >= 20)
            {
                checkInterval = 0;
                if (pMomentum.Count >= 30)
                    pMomentum.RemoveAt(0);
                pMomentum.Add(rMomentum);
            }
        }

        if (firing)
        {
            fShake = 0.05f;
            //Vector3 pos = cam.transform.localPosition;
            //pos.y = 1 + pHDist;
            //Debug.Log(pos);
            //cam.transform.localPosition = pos;

            //Vector3 rot = cam.transform.localEulerAngles;
            //rot.x = pHAngle;
            //cam.transform.localEulerAngles = rot;
        }

        if (shake > 0 || fShake > 0)
        {
            float tshake = 0;

            if (firing && fShake > 0)
                tshake += fireShake;
            if (playerHit && shake > 0)
                tshake += shakeAmt;

            Vector3 cTemp = UnityEngine.Random.insideUnitSphere * tshake;

            if (firing && !playerHit)
            { cTemp.x = 0; }

            cTemp.y += 1;
            cTemp.z = -1.75f;
            cam.transform.localPosition = cTemp;
            shake -= Time.deltaTime * decrement;
            fShake -= Time.deltaTime * decrement;

        }
        else if (playerHit)
        {
            playerHit = false;
            Time.timeScale = 1;
        }
    }

    bool DashChecker(KeyCode k, int i)
    {

        if (Input.GetKeyDown(k))
        {
            if (!DashCheck[i])
            {
                DashCheck[i] = true;
                dashTimer = 0.25f;
                return false;
            }
            else if (dashTimer > 0)
            {
                DashCheck[i] = false;
                dashing = true;
                dashDuration = 1.0f;
                return true;
            }
            else
                DashCheck[i] = false;
        }
        return false;
    }

	void OnCollisionEnter (Collision col)
	{
		if(col.gameObject.tag == "projectile")
		{
			float dmg = col.gameObject.GetComponent<ProjectileMotion>().getDamage();
			currentHealth -= dmg;
            col.gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(0, 0, 0);
            col.gameObject.GetComponent<ProjectileMotion>().setShit(0,0);
            StartCoroutine(whiteFlash(col.gameObject));
			Destroy (col.gameObject, 0.075f);
            if(dashing)
            {
                dashDuration = 0;
            }
            //pHAngle = -10;
            pHDist = 0.25f;
            playerHit = true;

            shake = shakeAmt;

            rMomentum = rMomentum * 0.6f;
            zMomentum = Mathf.Clamp(zMomentum - dmg / 50, 0, radialSpeed); 	//knockback - can only slow player's forward movement, can't push them backwards.
            Time.timeScale = 0.5f; 
        }
	}

    IEnumerator whiteFlash(GameObject g)
    {
        yield return new WaitForSeconds(0.025f);
        g.GetComponent<ProjectileMotion>().setAnimator(blankAnim);
        g.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1);
        yield return null;
    }

	float AccelerateTowards(float current, float target, float speed)
	{
		float result = 0;
		if(current > target)
		{
			result = current - speed;
			if(result < target) result = target;
		}
		else
		{
			result = current + speed;
			if(result > target) result = target;
		}
		return result;
	}

	Vector3 AccelerateTowardsV3(Vector3 current, Vector3 target, int speed)
	{
		Vector3 newM;
		newM.x = AccelerateTowards (current.x, target.x, speed);
		newM.y = AccelerateTowards (current.y, target.y, speed);
		newM.z = AccelerateTowards (current.z, target.z, speed);

		return newM;
	}

    public bool getFiring()
    { return firing; }

    public void setFiring(bool f)
    { firing = f; }

    void setFacing(int i)
    {
        if (!getFiring())
        {
            sprite.GetComponent<SpriteRenderer>().sprite = facings[i];
            sprite.GetComponent<Animator>().runtimeAnimatorController = anims[i];
        }
        else
        {
            sprite.GetComponent<Animator>().runtimeAnimatorController = shootAnims[i];
        }
    }

    public float getTrackingMomentum(float pSpeed)
    {

        float trackAmt = 0;
        pSpeed = pRadius.z/pSpeed;
        int start = pMomentum.Count - (int)(pSpeed * 3);
        if (start <= 0) start = 0;
        start = pMomentum.Count - 3;
        for (int i = start; i < pMomentum.Count; i++)
        {
            trackAmt += pMomentum[i];
        }
        //Debug.Log(trackAmt);
        trackAmt /= (pMomentum.Count-start);
        return trackAmt;
    }

    public float getRadius()
    {
        return (pRadius.z*-1);
    }

    void loadPlayerStats()
    {
        try
        {
            string file = Application.dataPath;
            file = file + "/Resources/" + Globals.getLevel() + "/PlayerStats.txt";
            StreamReader theReader = new StreamReader(file, Encoding.Default);

            using (theReader)
            {
                theReader.ReadLine(); //MOVE SPEED //accel = fraction of max.
                radialSpeed = float.Parse(theReader.ReadLine().Split(' ')[1]); //Forward 6
                zAccel = float.Parse(theReader.ReadLine().Split(' ')[1]); //FAccel 0.033
                rotSpeed = int.Parse(theReader.ReadLine().Split(' ')[1]); //Radial 600
                rAccel = float.Parse(theReader.ReadLine().Split(' ')[1]); //RAccel 0.05
                theReader.ReadLine();                       //
                theReader.ReadLine();                       //CONSTRAINTS
                minDist = int.Parse(theReader.ReadLine().Split(' ')[1]);  //MinRange 5
                maxDist = int.Parse(theReader.ReadLine().Split(' ')[1]);  //MaxRange 30
                pRadius = gameObject.transform.localPosition;
                pRadius.z = -1 * int.Parse(theReader.ReadLine().Split(' ')[1]);//Start 10
                theReader.ReadLine();
                theReader.ReadLine();//HEALTH
                maxHealth = int.Parse(theReader.ReadLine().Split(' ')[1]);//Max 15000
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

    public int getMD()
    {
        return maxDist;
    }

    public bool moveKeyCheck(string key)
    {
        if (Globals.getKeySetting() == 2)
        {
            switch (key)
            {
                case "W":
                    return Input.GetKey(KeyCode.I);
                case "A":
                    return Input.GetKey(KeyCode.J);
                case "S":
                    return Input.GetKey(KeyCode.K);
                case "D":
                    return Input.GetKey(KeyCode.L);
            }
        }
        else
            switch (key)
            {

                case "W":
                    return Input.GetKey(KeyCode.W);
                case "A":
                    return Input.GetKey(KeyCode.A);
                case "S":
                    return Input.GetKey(KeyCode.S);
                case "D":
                    return Input.GetKey(KeyCode.D);
            }
        return false;
    }
}
