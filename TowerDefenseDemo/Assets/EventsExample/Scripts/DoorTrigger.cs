using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public int doorID;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            EventManager.OpenDoorWithID(doorID);
        }
    }
}
