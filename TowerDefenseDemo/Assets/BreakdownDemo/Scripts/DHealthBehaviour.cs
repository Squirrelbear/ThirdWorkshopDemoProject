using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DHealthBehaviour : MonoBehaviour
{
    public event Action<int, int, int> UnitHealthChanged;
    public event Action UnitDied;

    public int healthCurrent;
    public int healthMax;
    public bool invulnerable = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isDead()
    {
        return healthCurrent <= 0;
    }

    public void changeHealthBy(int amount)
    {
        if(invulnerable || amount == 0 || isDead() || (amount > 0 && healthCurrent == healthMax))
        {
            return;
        }

        if(amount < 0 && healthCurrent < Math.Abs(amount))
        {
            amount = -healthCurrent;
        } else if(amount > 0 && amount > healthMax - healthCurrent)
        {
            amount = healthMax - healthCurrent;
        } 
        
        healthCurrent += amount;
        UnitHealthChanged?.Invoke(healthCurrent, healthMax, amount);
        
        if(isDead())
        {
            UnitDied?.Invoke();
        }
    }
}
