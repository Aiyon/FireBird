using UnityEngine;
using System.Collections;

public class ProjectileMotion : MonoBehaviour {

	float speed = 0;
	int damage = 0;
    int alter;
    public string type;

    public GameObject sprite;
    public RuntimeAnimatorController[] facings;

    public GameObject player;

	// Use this for initialization
	void Start ()
    {
        alter = 0;
	}

    // Update is called once per frame
    void Update()
    {
        if (Globals.paused) return;

        transform.position -= transform.forward * speed * Time.deltaTime;

        //		Vector3 pos = gameObject.transform.localPosition;
        //		pos.z -= speed * Time.deltaTime;
        //		gameObject.transform.localPosition = pos;

        int iT = player.transform.GetChild(0).gameObject.GetComponent<PlayerController>().getMD();
        float temp = Mathf.Abs(player.transform.GetChild(0).localPosition.z) + 5; if (temp > iT) temp = iT; //hard cap on delete range
        if (Mathf.Abs(transform.localPosition.z) > temp) //> 32)
            Destroy(gameObject);

        if (player != null)
        {
            Vector3 rot = player.transform.rotation.eulerAngles; rot.z *= -1;
            sprite.transform.rotation = Quaternion.Euler(rot);
        }

        if (type.ToLower() == "homing")
        {
            float pTemp = player.transform.GetChild(0).localPosition.magnitude;
            //course correct
            float projAbs = gameObject.transform.localPosition.magnitude;
            if (alter == 0)
            {

                if (projAbs >= (pTemp * 0.33f))
                {
                    alter = 1;
                }
            }
            else if (projAbs >= (pTemp * 0.6f))
            {
                Debug.Log(alter);
                alter = 2;
            }
        }
        if (type.ToLower() == "breakable" && sprite.transform.localEulerAngles.y < 20 && player.GetComponentInChildren<PlayerController>().getFiring())
        {
            //Vector3 pTemp = player.transform.GetChild(0).position.normalized;
            //Vector3 projAngle = gameObject.transform.position.normalized;

            float t = -90 / (Mathf.PI * player.transform.GetChild(0).position.z);
            if (Mathf.Abs(sprite.transform.localEulerAngles.y) <= t)
            {
                Debug.Log("boom");
                Destroy(gameObject);
            }

        }

        if (alter == 1)
        {
            Vector3 pos = transform.position - player.transform.GetChild(0).position;
            var newRot = Quaternion.LookRotation(pos);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 0.1f);
        }

        float tempR = sprite.transform.localEulerAngles.y; tempR *= -1;
        if (tempR > 180) tempR -= 360;
        if (tempR < -180) tempR += 360;

        float absR = Mathf.Abs(tempR);

        if (absR == 0) setFace(0);
        else if (absR < 10) setFace(1);
        else if (absR < 20) setFace(2);
        else if (absR < 30) setFace(3);
        else if (absR < 45) setFace(4);
        else if (absR < 90) setFace(5);

        if (tempR < 0)
        {
            setFlip(false);
        }
        else setFlip(true);
    }


    public void setPlayer(GameObject gObj)
    {
        player = gObj;
    }

    void setFace(int i)
    {
        sprite.GetComponent<Animator>().runtimeAnimatorController = facings[i];
    }

    void setFlip(bool f)
    {
        Vector3 s = sprite.transform.localScale;
        if (f)
        {
            s.x = -4;
            }
        else s.x = 4;

        sprite.transform.localScale = s;
    }

    public void setShit(int dmg, float spd)
    {
        damage = dmg;
        speed = spd;
    }

    public int getDamage()
    {
        return damage;
    }

    public string getType()
    {
        return type;
    }

}
