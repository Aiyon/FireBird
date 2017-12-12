using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rangeFinderUpdate : MonoBehaviour {

    public GameObject player;
    public Sprite[] ranges;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        Vector3 t = gameObject.transform.position;
        if (Input.GetKey(KeyCode.Q))
            t.x -= Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            t.x += Time.deltaTime;

        gameObject.transform.position = t;
    }
}
