using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DDebuffManagerBehaviour : MonoBehaviour
{
    public List<DDebuffBase> debuffList;

    // Start is called before the first frame update
    void Start()
    {
        debuffList = new List<DDebuffBase>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(DDebuffBase debuff in debuffList)
        {
            debuff.updateDuration();
            if(debuff.isExpired())
            {
                debuff.removeEffect(gameObject);
                debuffList.Remove(debuff);
            }
        }
    }

    public void applyDebuff(DDebuffBase debuff)
    {
        // Don't add if it already exists.
        if(debuffList.Where(d => d.name == debuff.name).Count() != 0)
        {
            debuffList.Add(debuff);
            debuff.applyEffect(gameObject);
        }
    }
}
