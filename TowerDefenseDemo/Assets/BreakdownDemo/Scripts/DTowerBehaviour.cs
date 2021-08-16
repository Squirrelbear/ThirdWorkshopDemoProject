using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DTowerBehaviour : MonoBehaviour
{
	public float shootCooldown = 0.5f;
	public float maxRange = 5;
	public Transform firingPosition;
	public GameObject projectilePrefab;

	public GameObject currentTarget;
	public bool isShooting = true;
	public bool requireTargetToShoot = true;
	public float timeSinceLastShot;

	public event Action OnSelectedTower;
	public event Action OnDeselectedTower;

	void Awake()
	{
		if (firingPosition == null)
		{
			firingPosition = transform;
		}
	}

	void Start()
	{

	}

	bool selected = false;
    private void OnMouseDown()
    {
        if(selected)
        {
			OnDeselectedTower?.Invoke();
        } else
        {
			OnSelectedTower?.Invoke();
        }
		selected = !selected;
    }

    void Update()
	{
		updateShooting();
	}

	public void setIsShooting(bool isShooting)
	{
		this.isShooting = isShooting;
	}

	private void updateShooting()
	{
		timeSinceLastShot += Time.deltaTime;
		if (timeSinceLastShot > shootCooldown)
		{
			timeSinceLastShot -= shootCooldown;
			// if the target does not exist, or the target is too far away, or if the target can no longer be damaged.
			if (currentTarget == null ||
			Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) > maxRange
				|| !currentTarget.GetComponent<BoxCollider>().enabled)
			{
				currentTarget = findNextTarget();
			}
			fireProjectile();
		}
	}

	private void fireProjectile()
	{
		if (currentTarget == null && requireTargetToShoot) return;
		var projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
		projectile.transform.parent = gameObject.transform;
		DProjectileBehaviour projectileBehaviour = projectile.GetComponent<DProjectileBehaviour>();
		projectileBehaviour.setTarget(currentTarget, firingPosition);
	}

	private GameObject findNextTarget()
	{
		GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
		GameObject bestTarget = null;
		float bestTargetDistance = maxRange + 10000;
		foreach (GameObject target in targets)
		{
			float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
			if (distance < bestTargetDistance && distance < maxRange && target.GetComponent<BoxCollider>().enabled)
			{
				bestTarget = target;
				bestTargetDistance = distance;
			}
		}
		return bestTarget;
	}
}
