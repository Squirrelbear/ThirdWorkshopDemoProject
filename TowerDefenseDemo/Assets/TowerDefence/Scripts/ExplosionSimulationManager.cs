using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionSimulationManager : MonoBehaviour {

    public List<RockMoveBehaviour> rocks;
    public List<Transform> finishPoints;

    public GameObject explosionPrefab;
    public Vector3 prefabSpawnPosition;

	// Use this for initialization
	void Start () {
        rocks = new List<RockMoveBehaviour>();
        finishPoints = new List<Transform>();
        foreach (Transform c in transform)
        {
            if (c.name.StartsWith("Rock"))
            {
                rocks.Add(c.GetComponent<RockMoveBehaviour>());
            }
            else if(c.name.Equals("FinishLocations"))
            {
                foreach (Transform a in c.transform)
                {
                    finishPoints.Add(a);
                }
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.J))
        {
            explode();
        }
        else if(Input.GetKeyDown(KeyCode.L))
        {
            resetRocks();
        }
	}

    public void explode()
    {
        Instantiate(explosionPrefab, prefabSpawnPosition, Quaternion.identity);
        for(int i = 0; i < rocks.Count; i++)
        {
            rocks[i].beginTossTo(finishPoints[i].position);
        }
    }

    public void resetRocks()
    {
        for (int i = 0; i < rocks.Count; i++)
        {
            rocks[i].resetPosition();
        }
    }
}
