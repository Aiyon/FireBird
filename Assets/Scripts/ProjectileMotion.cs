using UnityEngine;
using System.Collections;

public class ProjectileMotion : MonoBehaviour {

	float speed = 0;
	int damage = 0;

    public GameObject sprite;
    public Sprite[] facings;

    GameObject player;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position -= transform.forward * speed * Time.deltaTime;

        //		Vector3 pos = gameObject.transform.localPosition;
        //		pos.z -= speed * Time.deltaTime;
        //		gameObject.transform.localPosition = pos;

        float temp = Mathf.Abs(player.transform.GetChild(0).localPosition.z) + 5; if (temp > 32) temp = 32;
        if (Mathf.Abs(transform.localPosition.z) > temp) //> 32)
            Destroy (gameObject);

        if (player != null)
        {
            Vector3 rot = player.transform.rotation.eulerAngles; rot.z *= -1;
            sprite.transform.rotation = Quaternion.Euler(rot);
        }

        float tempR = sprite.transform.localEulerAngles.y; tempR *= -1;
        if (tempR > 180) tempR -= 360;
        if (tempR < -180) tempR += 360;

        if (tempR <= -90) setFace(6);
        else if (tempR <= -60) setFace(5);
        else if (tempR <= -30) setFace(4);
        else if (tempR >= 90) setFace(0);
        else if (tempR >= 60) setFace(1);
        else if (tempR >= 30) setFace(2);
        else setFace(3);
    }

    public void setPlayer(GameObject gObj)
    {
        player = gObj;
    }

    void setFace(int i)
    {
        sprite.GetComponent<SpriteRenderer>().sprite = facings[i];
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
}
