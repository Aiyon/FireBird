using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{

    public GameObject cam;
    bool loading;

    public void Start()
    {
        loading = false;
    }

    public void Update()
    {
        if (loading)
        {
            float r = -6.3f * Time.deltaTime;
            Debug.Log(r);
            if (cam.transform.rotation.x > 0)
            {
                cam.transform.Rotate(new Vector3(r, 0, 0));
            }
            else
            {
                Vector3 rot = cam.transform.rotation.eulerAngles;
                rot.x = 0;
                cam.transform.eulerAngles = rot;
                loading = false;
            }
        }
    }

    public void levelSelect(int i)
    {
        StartCoroutine(levelWindup(i));
        loading = true;
    }

    public void qq()
    {
        Application.Quit();
    }

    public IEnumerator levelWindup(int i)
    {
        yield return new WaitForSeconds(2);
        Globals.setLevel("/L" + i);
        loading = false;
        SceneManager.LoadScene(i);
        yield return null;
    }
}
