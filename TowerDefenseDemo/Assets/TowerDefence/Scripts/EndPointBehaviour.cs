using UnityEngine;
using System.Collections;

public class EndPointBehaviour : MonoBehaviour {

    public float maxUnitHealth = 25;
    public float unitHealth = 25;
    public Texture2D healthBarTexture;
    private HealthBarBehaviour healthBar;

    public GameObject gameOverObject;

	// Use this for initialization
	void Start () {
        healthBar = gameObject.GetComponentInChildren<HealthBarBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void resetHealth()
    {
        //print("end point health reset");
        unitHealth = maxUnitHealth;
        healthBar.setHealth(unitHealth, maxUnitHealth); 
    }

    public void applyDamage(float amount)
    {
        unitHealth -= amount;
        if (unitHealth <= 0)
        {
            unitHealth = 0;
            if (gameOverObject != null)
            {
                gameOverObject.SetActive(true);
            }
        }
        healthBar.setHealth(unitHealth, maxUnitHealth);
    }
}
