using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DHealthBehaviour))]
public class DMoveToTargetBehaviour : MonoBehaviour
{
    public event Action TargetReached;

    public GameObject currentWaypoint;
    public float moveSpeed = 5;
    public float rotationSpeed = 3;
    public float targetReachedThreshold = 0.4f;
    public AnimationClip walkAnimation;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<DHealthBehaviour>().UnitDied += clearWaypoint;
    }

    // Update is called once per frame
    void Update()
    {
        moveTowardWaypoint();
    }

    public void clearWaypoint()
    {
        currentWaypoint = null;
    }

    public void multiplySpeedBy(float multiplier)
    {
        moveSpeed *= multiplier;
    }

    public void moveTowardWaypoint()
    {
        if (currentWaypoint == null) return;

        // Turn toward the target point and then move foward
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                Quaternion.LookRotation(currentWaypoint.transform.position - transform.position),
                                rotationSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Play the walk animation if one is set
        if(walkAnimation != null)
        {
            Animation anim = gameObject.GetComponentInChildren<Animation>();
            if (!anim.isPlaying)
            {
                anim.Play(walkAnimation.name);
                anim.wrapMode = WrapMode.Loop;
            }
        }

        // Check if the unit has reached the next waypoint
        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), 
                                            new Vector2(currentWaypoint.transform.position.x, currentWaypoint.transform.position.z));
        if (distance < targetReachedThreshold)
        {
            beginNextWaypoint();
        }
    }

    private void beginNextWaypoint()
    {
        if (currentWaypoint != null)
        {
            currentWaypoint = currentWaypoint.GetComponent<WayPointBehaviour>().nextWaypoint;
            if(currentWaypoint == null)
            {
                TargetReached?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
