using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DProjectileBehaviour : MonoBehaviour
{
    public enum ProjectilePathType { Direct, Swarm, NoMove }
    public enum ProjectileDamageType { NoTarget, SingleTarget, AOERadius }
    public enum DebuffTypes { None, Slow }

    public ProjectilePathType projectilePathType;
    public ProjectileDamageType projectileDamageType;
    public DebuffTypes appliesDebuff;
    public int damageAmount;
    public int aoeDamageRadius;
    public GameObject target;
    public float rotationSpeed;
    public float moveSpeed;

    // Specific for the NoMove type.
    public bool noMoveApplied = false;
    public float destroyTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (projectilePathType == ProjectilePathType.NoMove)
        {
            if (!noMoveApplied)
            {
                noMoveApplied = true;
                applyImpact();
                destroyTimer = 3.5f;
            }
            destroyTimer -= Time.deltaTime;
            if(destroyTimer <= 0)
            {
                Destroy(gameObject);
            }
            return;
        }
        updateMovement();

        if (Vector3.Distance(transform.position, target.transform.position) < 0.5f)
        {
            applyImpact();
            Destroy(gameObject);
        }
    }
    public void setTarget(GameObject target, Transform startPoint)
    {
        this.target = target;
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
    }

    private void updateMovement()
    {
        if (projectilePathType == ProjectilePathType.Swarm)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                    Quaternion.LookRotation(target.transform.position - transform.position),
                                    rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void applyImpact()
    {
        if (projectileDamageType == ProjectileDamageType.SingleTarget)
        {
            applyToTarget(target);
        }
        else
        {
            Vector3 targetPos = projectileDamageType == ProjectileDamageType.NoTarget 
                                    ? transform.position : target.transform.position;
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                if (Vector3.Distance(targetPos, enemy.transform.position) < aoeDamageRadius)
                {
                    applyToTarget(enemy);
                }
            }
        }
    }

    private void applyToTarget(GameObject obj)
    {
        if (obj == null) return;

        DHealthBehaviour healthBehaviour = obj.GetComponent<DHealthBehaviour>();
        if (healthBehaviour != null)
        {
            healthBehaviour.changeHealthBy(damageAmount);
        }
        if (appliesDebuff == DebuffTypes.Slow)
        {
            DDebuffManagerBehaviour debuffManager = obj.GetComponent<DDebuffManagerBehaviour>();
            if (debuffManager != null)
            {
                debuffManager.applyDebuff(new DDebuffSlow(0.5f, 4));
            }
        }
    }
}
