using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerBehaviour : MonoBehaviour {

    public static float[,] towerModStats = new float[,] { { 20, 40, 0.5f }, { 0.5f, 25, 1 }, { 20, 30, 0.2f }, { 30, 30, 0.8f } };

    public enum TowerType { Basic = 0, Swarm = 1, Sniper = 2, Explosive = 3, Frost = 4 };

    public enum TowerState { TowerPlacement, TowerAIControlled, TowerInactive };

    // Tower properties
    public TowerType towerType;
    public float shootCooldown = 0.5f;
    public float maxRange = 5;
    public float towerDamage = 1;
    public float collidableDamageBarrier = 0.5f;

    // Projectile properties
    public Transform firingPosition;
    public GameObject projectilePrefab;
    public List<GameObject> offProjectiles;
    public List<GameObject> onProjectiles;

    // Tower state
    public GameObject currentTarget;
    public GameObject playerTarget;
    public TowerState towerState;
    public Material mat;
    public bool matAlphaIncreasing = false;

    // User control point
    public Transform cameraControlPoint;

    // Camera reference
    private CameraBehaviour cameraRef;

    // Timers
    private float timeSinceLastShot;

    // Child references
    public GameObject towerRange;
    private TowerRangeBehaviour towerRangeScript;

    private GameObject frostTowerEffect;

    void Awake()
    {
        onProjectiles = new List<GameObject>();
        offProjectiles = new List<GameObject>();
        if (towerType != TowerType.Frost)
        {
            for (int i = 0; i < 10; i++)
            {
                offProjectiles.Add(spawnProjectile());
            }
        }
        firingPosition = transform;

        timeSinceLastShot = 0;
        if (towerType == TowerType.Frost)
        {
            frostTowerEffect = transform.Find("FrostTowerAttack").gameObject;
        }
    }

	// Use this for initialization
	void Start () {
        cameraControlPoint = transform.Find("CamPoint");
        towerRangeScript = gameObject.GetComponentInChildren<TowerRangeBehaviour>();
        if (towerRangeScript != null)
        {
            towerRange = towerRangeScript.gameObject;
        }

        mat = GetComponentInChildren<MeshRenderer>().material;
        configCustomStats();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        if (cameraRef == null)
        {
            cameraRef = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
        }

        if (towerState == TowerState.TowerAIControlled)
        {
            timeSinceLastShot += deltaTime;
            if (timeSinceLastShot > shootCooldown)
            {
                timeSinceLastShot -= shootCooldown;
                if (currentTarget == null ||
                Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) > maxRange
                    || currentTarget.GetComponent<BoxCollider>().enabled != true)
                {
                    currentTarget = findNextTarget();
                }
                fireProjectile();
            }
        }

        if (towerRangeScript != null)
        {
            towerRangeScript.setRange(maxRange);
            towerRange = towerRangeScript.gameObject;
        }
        else
        {
            towerRangeScript = gameObject.GetComponentInChildren<TowerRangeBehaviour>();
            if (towerRangeScript != null)
            {
                towerRange = towerRangeScript.gameObject;
            }
        }

        if (cameraRef.selectedTower == this)
        {
            towerRange.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            towerRange.GetComponent<MeshRenderer>().enabled = false;
        }

        // Fixes error on updating projectiles when objects are removed from onProjectiles mid-update
        GameObject[] tempProjectilesToUpdate = new GameObject[onProjectiles.Count];
        onProjectiles.CopyTo(tempProjectilesToUpdate);

        foreach (GameObject projectile in tempProjectilesToUpdate)
        {
            projectile.GetComponent<ProjectileBehaviour>().update(deltaTime);
        }
	}

    private void fireProjectile()
    {
        // Don't fire if the tower is completely destroyed.
        if(towerType == TowerType.Frost)
        {
            slowAllTargetsInRange(maxRange, towerDamage);
        }
        else if (currentTarget != null)
        {
            GameObject projectile = null;
            if(offProjectiles.Count == 0) {
                projectile = spawnProjectile();
            }
            else
            {
                projectile = offProjectiles[0];
                offProjectiles.RemoveAt(0);
            }
            onProjectiles.Add(projectile);
            ProjectileBehaviour projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();
            projectileBehaviour.setTarget(currentTarget, firingPosition);
        }
    }

    private void slowAllTargetsInRange(float range, float speedMultiplier)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject target in targets)
        {
            if (Vector3.Distance(target.transform.position, gameObject.transform.position) < range)
            {
                AIWayfinder targetScript = target.GetComponent<AIWayfinder>();
                if (targetScript != null)
                {
                    targetScript.applySpeedModifier(speedMultiplier, shootCooldown * 4);
                }
            }
        }
    }

    private GameObject findNextTarget(GameObject thatIsNotThis = null)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestTargetDistance = maxRange+10000;
        foreach(GameObject target in targets) 
        {
            float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
            if(target != thatIsNotThis && distance < bestTargetDistance && distance < maxRange && target.GetComponent<BoxCollider>().enabled == true)
            {
                bestTarget = target;
            }
        }
        return bestTarget;
    }

    private GameObject spawnProjectile()
    {
        var projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        projectile.transform.parent = gameObject.transform;
        ProjectileBehaviour script = projectile.GetComponent<ProjectileBehaviour>();
        script.setTower(gameObject);
        script.destroyProjectile();
        return projectile;
    }

    public void resetProjectile(GameObject projectile)
    {
        onProjectiles.Remove(projectile);
        offProjectiles.Add(projectile);
    }

    public void setTowerState(TowerState state)
    {
        towerState = state;

        if (towerState != TowerState.TowerPlacement)
        {
            Color newColor2 = mat.color;
            newColor2.a = 1;
            mat.color = newColor2;
        }
    }

    public void destroyTower()
    {
        foreach(GameObject obj in onProjectiles)
        {
            Destroy(obj);
        }

        foreach(GameObject obj in offProjectiles)
        {
            Destroy(obj);
        }
    }

    private void configCustomStats()
    {
        int modIndex = 0;
        switch(towerType)
        {
            case TowerType.Basic:
                modIndex = 0;
                break;
            case TowerType.Frost:
                modIndex = 1;
                break;
            case TowerType.Swarm:
                modIndex = 2;
                break;
            case TowerType.Explosive:
                modIndex = 3;
                break;
        }

        towerDamage = towerModStats[modIndex, 0];
        maxRange = towerModStats[modIndex, 1];
        shootCooldown = towerModStats[modIndex, 2];
    }
}
