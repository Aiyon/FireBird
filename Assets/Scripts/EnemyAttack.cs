using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class EnemyAttack : MonoBehaviour {

	public GameObject[] projectile;
    public GameObject bullet;
    private Vector3 b_velocity;
    private Vector3 b_force;
    bool bulletFront;
    bool bulletRight;
    bool bulleting;
    int bulletID;
    float bulletCD;
    float bulletDuration;

    public GameObject player;
	bool attacking;
	int atkNum;
	float atkCounter;
    float atkDuration;
    public bool chase;

    //pattern lists 
    private List<Pattern> patternListShort;
    private List<Pattern> patternListMedium;
    private List<Pattern> patternListLong;
    private List<Pattern> patternListXLong;

    private Pattern currentAttack;
    private int currentWave;

    private List<int> types = new List<int>();
    private List<float> speeds = new List<float>();
    private List<int> damages = new List<int>();

    private float shortMax;
    private float medMax;
    private float largeMax;

    private int patternOffset;
    private Quaternion rotProj;

    // Use this for initialization
    void Start ()
	{
		attacking = false;
		atkCounter = 0;

        string file = Application.dataPath;
        file = file + "/Resources" + Globals.getLevel() + "/Patterns";

        //load pattern sets.
        patternListShort = new List<Pattern>();
        loadPatterns(file + "/SPatterns.txt", patternListShort);

        patternListMedium = new List<Pattern>();
        loadPatterns(file + "/MPatterns.txt", patternListMedium);

        patternListLong = new List<Pattern>();
        loadPatterns(file + "/LPatterns.txt", patternListLong);

        patternListXLong = new List<Pattern>();
        loadPatterns(file + "/XLPatterns.txt", patternListXLong);

        loadProjs();

        bulletFront = false;
        bulletRight = false;
        bulleting = false;
        bulletID = 0;
        bulletCD = 10;
        bulletDuration = 10.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (Globals.paused) return;
        //if(Input.GetKeyDown(KeyCode.F))
        //{
        //	Vector3 vProj = gameObject.transform.position;
        //	vProj.y = 0.5f;
        //	Instantiate(projectile[0], vProj, gameObject.transform.rotation);
        //}
        //if(Input.GetKeyDown(KeyCode.G))
        //{
        //	Vector3 vProj = gameObject.transform.position;
        //	vProj.y = 0.5f;
        //          Instantiate(projectile[1], vProj, gameObject.transform.rotation);

        //}

        if(bulletCD > 0)
        {
            bulletCD -= Time.deltaTime;
        }
        else
        {
            if (!bulleting && Mathf.Abs(player.transform.GetChild(0).localPosition.z) > shortMax)
            {
                bulleting = true;
                //bullet things
                int bRand = UnityEngine.Random.Range(0, 4);
                switch(bRand)
                {
                    case 1:
                        bulletFront = true;
                        bulletRight = false;
                        break; 
                    case 2:
                        bulletFront = true;
                        bulletRight = true;
                        break;
                    case 3:
                        bulletFront = false;
                        bulletRight = false;
                        break;
                    default:
                        bulletFront = false;
                        bulletRight = true;
                        break;

                }
                bulletDuration = 10;
            }
            else
            {
                bulletDuration -= Time.deltaTime;
                if (bulletDuration <= 0)
                {
                    bulletCD = 20.0f;
                    bulleting = false;
                }
            }
        }

        if (!attacking)
		{
			atkCounter = 0;
            currentWave = 0;

            float pDist = Mathf.Abs(player.transform.GetChild(0).localPosition.z);

            if (pDist < shortMax)
            {
                atkNum = UnityEngine.Random.Range(0, patternListShort.Count);
                currentAttack = patternListShort[atkNum];
            }
            else if (pDist < medMax)
            {
                atkNum = UnityEngine.Random.Range(0, patternListMedium.Count);
                currentAttack = patternListMedium[atkNum];
            }
            else if (pDist < largeMax)
            {
                atkNum = UnityEngine.Random.Range(0, patternListLong.Count);
                currentAttack = patternListLong[atkNum];
            }
            else
            {
                atkNum = UnityEngine.Random.Range(0, patternListXLong.Count);
                currentAttack = patternListXLong[atkNum];
            }
            setPatternRot();
            attacking = true;
		}
		else
		{            
            if(atkCounter == 0)
            {
//                Debug.Log(currentWave + " / " + currentAttack.numWaves());
                string[] w = currentAttack.getWaves(currentWave).Split(',');
//                Debug.Log(currentAttack.getWaves(currentWave));

                int n = currentAttack.getType();
                if(n == 2)
                {
                    //do bullet things.
                }
                for (int i = 0; i<w.Length; i++)
                {
                    newProjectile(n, float.Parse(w[i]));
                }

                if (currentWave >= currentAttack.numWaves() - 1) atkCounter = currentAttack.getInterval();
                else atkCounter = currentAttack.getSpeed();
            }
            else
            {
                atkCounter -= Time.deltaTime;
                if (atkCounter <= 0)
                {
                    atkCounter = 0;
                    currentWave++;
                    if (currentWave >= currentAttack.numWaves()) attacking = false;
                }
            }
		}
        bulletSetter();

        ParticleSystem.EmissionModule em = bullet.GetComponent<ParticleSystem>().emission;
        em.enabled = bulleting;

        if (bulleting)
        {
            ParticleSystem p = bullet.GetComponent<ParticleSystem>();
            ParticleSystem.VelocityOverLifetimeModule v = p.velocityOverLifetime;
            v.x = new ParticleSystem.MinMaxCurve(b_velocity.x); v.z = new ParticleSystem.MinMaxCurve(b_velocity.z);
            ParticleSystem.ForceOverLifetimeModule f = p.forceOverLifetime;
            f.x = new ParticleSystem.MinMaxCurve(b_force.x);
            //bulleting = false;
        }
    }

    void setPatternRot()
    {
        rotProj = gameObject.transform.rotation;
        if (chase)
        {
            float lead = player.GetComponentInChildren<PlayerController>().getTrackingMomentum(speeds[currentAttack.getType()]);
            lead *= player.GetComponentInChildren<PlayerController>().getRadius();
            lead /= speeds[currentAttack.getType()];

            rotProj *= Quaternion.AngleAxis(lead, transform.up);
        }
    }

    void bulletSetter()
    {
        b_velocity.z = player.transform.GetChild(0).localPosition.z;

        if (bulletFront)
            b_velocity.z += 0.5f;
        else
            b_velocity.z -= 1.5f;

        b_velocity.x = b_velocity.z;
        if (bulletRight)
            b_velocity.x *= -1;

        b_force.x = b_velocity.x * -2;
    }

    void newProjectile(int p, float angle)
	{
		Vector3 vProj = gameObject.transform.position;
		vProj.y = 0.5f;
        Quaternion pRot = rotProj * Quaternion.AngleAxis(-angle, transform.up);
        GameObject proj = (GameObject)Instantiate(projectile[types[p]], vProj, pRot);
        proj.GetComponent<ProjectileMotion>().setPlayer(player);
        proj.GetComponent<ProjectileMotion>().setShit(damages[p], speeds[p]);
    }

    private void loadPatterns(string f, List<Pattern> l)
    {
        try
        {
            string line;
            StreamReader theReader = new StreamReader(f, Encoding.Default);

            using (theReader)
            {
                line = theReader.ReadLine();
                while (line != null)
                {
                    if (line.ToLower().StartsWith("pattern"))
                    {
                        line = theReader.ReadLine();
                        string[] temp = line.Split(',');
                        Pattern p = new Pattern(int.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
                        while (true)
                        {
                            line = theReader.ReadLine();
                            if (line.StartsWith("%P"))
                            {
                                break;
                            }
                            else
                            {
                                p.newWave(line);
                            }
                        }

                        l.Add(p);
                    }
                    line = theReader.ReadLine();
                }
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

    private void loadProjs()
    {
        try
        {
            string line;
            string file = Application.dataPath;
            file = file + "/Resources" + Globals.getLevel() + "/Projectiles.txt";
            StreamReader theReader = new StreamReader(file, Encoding.Default);

            using (theReader)
            {

                theReader.ReadLine();
                string[] dists = theReader.ReadLine().Split(',');

                shortMax = float.Parse(dists[0]);
                medMax = float.Parse(dists[1]);
                largeMax = float.Parse(dists[2]);

                theReader.ReadLine();
                theReader.ReadLine(); //skip format string.
                do
                {
                    line = theReader.ReadLine();
                    if (line != null)
                    {
                        string[] projParams = line.Split(',');
                        types.Add(int.Parse(projParams[0]));
                        damages.Add(int.Parse(projParams[1]));
                        speeds.Add(float.Parse(projParams[2]));

                    }
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