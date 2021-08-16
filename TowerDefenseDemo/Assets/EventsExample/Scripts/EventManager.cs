using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static event Action<int> OnDoorTriggerEvent;
    public static event Action OnDoorTriggerAllEvent;

    public static void OpenDoorWithID(int doorID)
    {
        OnDoorTriggerEvent?.Invoke(doorID);
    }

    public static void OpenAllDoors()
    {
        OnDoorTriggerAllEvent?.Invoke();
    }
}
