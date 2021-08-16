using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DHealthBehaviour))]
public class DDeathAnimator : MonoBehaviour
{
    public bool isDead = false;
    public float deathTime = 3;
    public float dragOnDeath = 5;
    public AnimationClip deathAnimation;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<DHealthBehaviour>().UnitDied += beginDeathAnimation;
    }

    // Update is called once per frame
    void Update()
    {
        // If the unit is dead drop it into the ground then despawn after a period of time.
        if (isDead)
        {
            deathTime -= Time.deltaTime;
            transform.position += new Vector3(0, 0, -0.05f) * Time.deltaTime;
            if (deathTime <= 0)
            {
                Destroy(this.gameObject);
            }
            return;
        }
    }

    public void beginDeathAnimation()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().drag = dragOnDeath;
        Animation anim = gameObject.GetComponentInChildren<Animation>();
        if (deathAnimation != null && anim != null)
        {
            anim.Play(deathAnimation.name);
        }
        isDead = true;
    }
}
