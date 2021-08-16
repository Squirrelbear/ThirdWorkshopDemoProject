using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Based on code from: https://www.youtube.com/watch?v=70PcP_uPuUc

    public Transform moveToPoint;
    public float openSpeed = 5;
    public int doorID;
    public bool open = false;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnDoorTriggerEvent += OpenDoorByID;
        EventManager.OnDoorTriggerAllEvent += OpenDoor;
    }

    // Update is called once per frame
    void Update()
    {
        if(open)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveToPoint.position, openSpeed * Time.deltaTime);
        }
    }

    private void OpenDoor()
    {
        open = true;
    }

    private void OpenDoorByID(int doorID)
    {
        if(this.doorID == doorID)
        {
            OpenDoor();
        }
    }

    private void OnDisable()
    {
        EventManager.OnDoorTriggerEvent -= OpenDoorByID;
        EventManager.OnDoorTriggerAllEvent -= OpenDoor;
    }
}
