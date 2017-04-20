using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	private Vector3 pRadius;
	private GameObject parent;
	public GameObject Enemy;

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
    float dashCool;

//    int equipped = 0;
    bool firing;

    bool outpacingL;    //can the enemy turn fast enough to keep up with the player?
    bool outpacingR;
    float OPTimer;

    public Sprite[] facings;
    //0 = idle, 1 = left, 2 = right, 3 = forward-left, 4 = forward-right, 5 = back-left, 6 = back-right
    public GameObject sprite;

    public Button menu;

    // Use this for initialization
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
        currentHealth = maxHealth;
        rMomentum = 0;
        outpacingL = outpacingR = false;
        OPTimer = 0;
        loadPlayerStats();

        Globals.paused = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        menu.gameObject.SetActive(Globals.paused);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Globals.paused = !Globals.paused;
        }
        if (Globals.paused)return;

        bool strafe = false;

		//healthSlider.value = currentHealth * healthSlider.maxValue / maxHealth;

        healthText.text = currentHealth + " AP ";
		//---

		adjSpeed = rotSpeed / pRadius.z; //adjusted for player distance
		if (adjSpeed < 0) adjSpeed *= -1;

		if(!Input.GetKey(KeyCode.W)&&!Input.GetKey(KeyCode.S))
		{
			zMomentum = AccelerateTowards(zMomentum, 0, radialSpeed/30);
		}

        if (!Input.GetKey(KeyCode.A)&& !Input.GetKey(KeyCode.D))
        {
            rMomentum = AccelerateTowards(rMomentum, 0, adjSpeed*rAccel*0.7f);
        }
        
			//CHECK IF PLAYER IS MOVING IN Z BEFORE DOING POLAR MOVE
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
				adjSpeed /= Mathf.Sqrt (2);

        //POLAR/ROTATION MOVE
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            strafe = true;
            rMomentum = AccelerateTowards(rMomentum, adjSpeed, adjSpeed*rAccel);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rMomentum = AccelerateTowards(rMomentum, adjSpeed * -1, adjSpeed*rAccel);
        }


		//RADIUS MOVE
		float tSpeed = radialSpeed;
		if(strafe) tSpeed /= Mathf.Sqrt(2);

        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            zMomentum = AccelerateTowards(zMomentum, tSpeed, tSpeed / 10);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            zMomentum = AccelerateTowards(zMomentum, tSpeed * -1, tSpeed / 10);
        }

		// MOMENTUM
		if (zMomentum != 0)
		{
			pRadius.z += zMomentum*Time.deltaTime;
			pRadius.z = Mathf.Clamp (pRadius.z, -maxDist, -minDist);
            if (zMomentum > tSpeed) zMomentum -= tSpeed / 10;
		}
		if(pRadius != gameObject.transform.localPosition)
		{
			gameObject.transform.localPosition = pRadius;
		}
		if(rMomentum != 0)
		{
			strafe = true;
			parent.transform.Rotate(0,rMomentum*Time.deltaTime,0);
			Enemy.transform.Rotate(0,rMomentum*Time.deltaTime,0);

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

        if (dashCool > 0) dashCool -= Time.deltaTime;
        else
        {
            if (DashChecker(KeyCode.W, 0))
                zMomentum = tSpeed * 2f;
            if (DashChecker(KeyCode.A, 1))
                rMomentum = adjSpeed * 1.5f;
            if (DashChecker(KeyCode.S, 2))
                zMomentum = tSpeed * -2f;
            if (DashChecker(KeyCode.D, 3))
                rMomentum = adjSpeed * -1.5f;
        }

        //set non-idle facings.
        if (rMomentum == 0)
            setFacing(0);
        else if(rMomentum > 0)  //clockwise aka left.
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

        if (OPTimer > 0 && (outpacingR||outpacingL))
        {
            OPTimer -= Time.deltaTime;
            if (OPTimer <= 0)
                Enemy.GetComponent<EnemyHealth>().spriteFacing(0);         
        }

        float tFloat = gameObject.transform.localPosition.z * -1000;
        int trueRange = (int)tFloat;
        rangeText.text = trueRange + "m";

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
                dashCool = 1;
                return true;
            }
            else DashCheck[i] = false;
        }
        return false;
    }

	void OnCollisionEnter (Collision col)
	{
		if(col.gameObject.tag == "projectile")
		{
			float dmg = col.gameObject.GetComponent<ProjectileMotion>().getDamage();
			currentHealth -= dmg;
			Destroy (col.gameObject);

            zMomentum = Mathf.Clamp(zMomentum - dmg / 50, 0, radialSpeed); 	//knockback - can only slow player's forward movement, can't push them backwards.
        }
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
        sprite.GetComponent<SpriteRenderer>().sprite = facings[i];
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
}
