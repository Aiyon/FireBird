using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern {

    private int type;
    private float speed;
    private float interval;
    private List<string> waves;


    public Pattern(int t, float s, float i)
    {
        type = t;
        speed = s;
        interval = i;
        waves = new List<string>();
    }

    public int getType()
    { return type; }

    public float getSpeed()
    { return speed; }

    public float getInterval()
    { return interval; }

    public void newWave(string wave)
    {
        waves.Add(wave);
    }

    public string getWaves(int i)
    {
        return waves[i];
    }
    public int numWaves()
    {
        return waves.Count;
    }

    ~Pattern()
    {
        waves.Clear();
    }

}
