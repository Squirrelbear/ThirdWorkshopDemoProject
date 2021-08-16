using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DTowerSnapPointBehaviour : MonoBehaviour
{
    public Vector3 snapPoint;
    public GameObject associatedTower;

    private const float actualTowerYPos = 1.59f;

    public GameObject towerChoiceReference;

    // Use this for initialization
    void Start()
    {
        snapPoint = new Vector3(transform.position.x, transform.position.y + actualTowerYPos, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool isAvailable()
    {
        return associatedTower == null;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(isAvailable() && other.tag == "Player")
        {
            TowerChoiceManager.callToSnapPoint(this);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            TowerChoiceManager.hide();
        }
    }
}
