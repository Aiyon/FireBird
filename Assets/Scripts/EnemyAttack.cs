﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class EnemyAttack : MonoBehaviour {

	public GameObject[] projectile;
    
    public GameObject player;
	bool attacking;
	int atkNum;
	float atkCounter;
    float atkDuration;

    private List<string> patternList;
    private List<int> types = new List<int>();
    private List<float> speeds = new List<float>();
    private List<int> damages = new List<int>();

    // Use this for initialization
    void Start ()
	{
		attacking = false;
		atkCounter = 0;

        patternList = new List<string>();
        loadPatterns();
        loadProjs();
	}
	
	// Update is called once per frame
	void Update ()
	{
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

		if(!attacking)
		{
			atkCounter = 0;
			atkNum = UnityEngine.Random.Range(0,patternList.Count);
			attacking = true;
		}
		else
		{            
            if(atkCounter == 0)
            {
                string[] pattern = patternList[atkNum].Split(',');
                int n = int.Parse(pattern[0]);
                if(n == 2)
                {
                    //do bullet things.
                }
                atkDuration = float.Parse(pattern[1]);
                for (int i = 2; i<pattern.Length; i++)
                {
                    newProjectile(n, float.Parse(pattern[i]));
                }

                atkCounter += Time.deltaTime;
            }
            else
            {
                atkCounter += Time.deltaTime;
                if (atkCounter >= atkDuration)
                    attacking = false;
            }
		}

	}
    
	void newProjectile(int p, float angle)
	{
		Vector3 vProj = gameObject.transform.position;
		vProj.y = 0.5f;
		Quaternion rotProj = gameObject.transform.rotation * Quaternion.AngleAxis (angle, transform.up);
        GameObject proj = (GameObject)Instantiate(projectile[types[p]], vProj, rotProj);
        proj.GetComponent<ProjectileMotion>().setPlayer(player);
        proj.GetComponent<ProjectileMotion>().setShit(damages[p], speeds[p]);
    }

    private void loadPatterns()
    {
        try
        {
            string line;
            string file = Application.dataPath;
            file = file + "/Resources" + Globals.getLevel() + "/Patterns.txt";
            StreamReader theReader = new StreamReader(file, Encoding.Default);

            using (theReader)
            {
                do
                {
                    line = theReader.ReadLine();
                    if(line!= null)
                        patternList.Add(line);
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