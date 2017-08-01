using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailSpriteController : MonoBehaviour {

    public GameObject player;
	
	// Update is called once per frame
	void Update ()
    {
        gameObject.transform.LookAt(Camera.main.transform.position, Vector3.up);

        Vector3 dist = gameObject.transform.position - player.transform.position;
        if (dist.magnitude < 0.5f)
        {
            Destroy(gameObject);
        }
    }

    public void setPlayer(GameObject p)
    {
        player = p;
    }
}
