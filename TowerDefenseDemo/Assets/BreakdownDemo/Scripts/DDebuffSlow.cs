using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDebuffSlow : DDebuffBase
{
    private float multiplier;

    public DDebuffSlow(float duration, float multiplier) : base("SlowDebuff", duration)
    {
        this.multiplier = multiplier;
    }

    public override void applyEffect(GameObject theObject)
    {
        theObject.GetComponent<DMoveToTargetBehaviour>().multiplySpeedBy(multiplier);
    }

    public override void removeEffect(GameObject theObject)
    {
        theObject.GetComponent<DMoveToTargetBehaviour>().multiplySpeedBy(1 / multiplier);
    }
}
