using UnityEngine;
using System.Collections;

public class AIWayfinder : MonoBehaviour
{
    public enum UnitType { Basic = 0, Fast = 1, Dangerous = 2, Boss = 3, MegaBoss = 4 };

    // Unit Information
    public UnitType unitType;
    public string unitName;

    // Navigation
    // Target Waypoint that can be modified
    public GameObject wayPoint;
    private Transform target;
    public WayPointBehaviour targetBehaviour;
    public float moveSpeed = 5;
    public float rotationSpeed = 3;
    public float targetReachThreshold = 0.4f;
    private Transform myTransform;
    public float moveSpeedMultiplier = 1;
    public float moveSpeedTimer = -1;

    // Unit Health
    private HealthBarBehaviour healthBar;
    public float maxUnitHealth = 5;
    public float unitHealth = 5;
    public float collidableDamageBarrier = 0.5f;
    public Texture2D healthBarTexture;

    // Death
    public bool isDead = false;
    public float deathTime = 0;
    public AnimationClip deathAnimation;
    private float timeTillDeath;

    // Aggressive properties
    public AnimationClip walkAnimation;
    public AnimationClip attackAnimation;

    void Awake()
    {
        myTransform = transform;
    }

	// Use this for initialization
	void Start() 
    {
        target = wayPoint.transform;
        unitHealth = maxUnitHealth;
        timeTillDeath = (unitType == UnitType.MegaBoss) ? 10 : 5;
        healthBar = gameObject.GetComponentInChildren<HealthBarBehaviour>();
        healthBar.setHealth(unitHealth, maxUnitHealth);
	}
	
	// Update is called once per frame
	public void update(float deltaTime) 
    {
        // If the unit is dead drop it into the ground then despawn after a period of time.
        if (isDead)
        {
            deathTime += deltaTime;
            myTransform.position += new Vector3(0, 0, -0.05f) * deltaTime;
            if (deathTime > timeTillDeath)
            {
                Destroy(this.gameObject);
            }
            return;
        }

        // Reached the end waypoint
        if (wayPoint == null)
        {
            Destroy(this.gameObject);
            return;
        }

        // Target not set or the waypoint has changed
        if(target == null || targetBehaviour == null || target.gameObject != wayPoint) 
        {
            updateWayPoint();
        }

        myTransform.rotation = Quaternion.Slerp(myTransform.rotation,
                                Quaternion.LookRotation(target.position - myTransform.position),
                                rotationSpeed * deltaTime);
        myTransform.position += myTransform.forward * moveSpeed * moveSpeedMultiplier * deltaTime;

        updateMoveSpeedMod(deltaTime);

        // Check if the unit has reached the next waypoint
        float distance = Vector2.Distance(new Vector2(myTransform.position.x, myTransform.position.z), new Vector2(target.position.x, target.position.z));
        if(distance < targetReachThreshold)
        {
            EndPointBehaviour script = target.GetComponent<EndPointBehaviour>();
            if (script != null)
            {
                // Reached the end, apply damage.
                script.applyDamage(1);
            }

            if(targetBehaviour != null)
            {
                wayPoint = targetBehaviour.nextWaypoint;
                updateWayPoint();
            }
            else
            {
                wayPoint = null;
            }
        }

        if (unitType == UnitType.Dangerous || unitType == UnitType.MegaBoss)
        {
            Animation anim = gameObject.GetComponentInChildren<Animation>();
            if (!anim.isPlaying)
            {
                anim.Play(walkAnimation.name);
                anim.wrapMode = WrapMode.Loop;
            }
        }
    }

    private void updateWayPoint() 
    {
        if (wayPoint == null)
        { 
            targetBehaviour = null;
            return;
        }

        target = wayPoint.transform;
        targetBehaviour = wayPoint.GetComponent<WayPointBehaviour>();
    }

    public void applyDamage(float amount)
    {
        if (isDead)
            return;

        unitHealth -= amount;
        healthBar.setHealth(unitHealth, maxUnitHealth);
        if (unitHealth <= 0)
        {
            isDead = true;
            // disable physics collisions
            healthBar.hide();
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().drag = (unitType == UnitType.Boss || unitType == UnitType.MegaBoss) ? 9 : 5;
            Animation anim = gameObject.GetComponentInChildren<Animation>();
            if (deathAnimation != null && anim != null)
            {
                anim.Play(deathAnimation.name);
            }
        }
    }

    public void applySpeedModifier(float speedAmount, float duration)
    {
        moveSpeedMultiplier = speedAmount;
        moveSpeedTimer = duration;
    }

    private void updateMoveSpeedMod(float deltaTime)
    {
        if(moveSpeedTimer == -1)
        {
            return;
        }

        moveSpeedTimer -= deltaTime;
        if (moveSpeedTimer <= 0)
        {
            moveSpeedTimer = -1;
            moveSpeedMultiplier = 1;
        }
    }
}
