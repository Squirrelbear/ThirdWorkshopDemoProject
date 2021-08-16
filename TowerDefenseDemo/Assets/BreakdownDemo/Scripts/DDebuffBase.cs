using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DDebuffBase
{
    public string name;
    public float debuffDuration;

    public DDebuffBase(string name, float duration)
    {
        this.name = name;
        this.debuffDuration = duration;
    }

    public void updateDuration()
    {
        debuffDuration -= Time.deltaTime;
    }

    public bool isExpired()
    {
        return debuffDuration <= 0;
    }

    public abstract void applyEffect(GameObject theObject);
    public abstract void removeEffect(GameObject theObject);
}
