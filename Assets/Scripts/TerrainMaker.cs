using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMaker : MonoBehaviour {

    public GameObject detailSprite;
    private Vector3 spawnPos;
    public GameObject player;

    // Use this for initialization
    void Start()
    {
        spawnPos.y = 0.08f;

        for (int i = 0; i < 150; i++)
        {
            spawnPos.x = Random.Range(-55.0f, 55.0f);
            spawnPos.z = Random.Range(-55.0f, 55.0f);
            GameObject g = (GameObject)Instantiate(detailSprite, spawnPos, Quaternion.identity);
            g.GetComponent<DetailSpriteController>().setPlayer(player);

            float theta = Random.Range(0, 360);
            int mesaLength = Random.Range(5, 10);
            for(int j = 0; j < mesaLength; j++)
            {
                //(x + 0.09 cos theta, y + 0.09 sin theta)
                float r = theta * 180 / Mathf.PI;
                spawnPos.x += (0.18f * Mathf.Cos(r));
                spawnPos.z += (0.18f * Mathf.Sin(r));
                g = (GameObject)Instantiate(detailSprite, spawnPos, Quaternion.identity);
                g.GetComponent<DetailSpriteController>().setPlayer(player);

                theta += Random.Range(-30.0f, 30.0f);
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
