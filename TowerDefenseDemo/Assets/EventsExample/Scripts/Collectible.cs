using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public event Action<Collectible> OnPickup;

    private Vector3 start;
    private bool increasing;
    public float yMaxChange;
    public float speed;

    // Start is called before the first frame update
    void OnEnable()
    {
        start = transform.position;
        increasing = UnityEngine.Random.Range(0, 1) == 0;
        Vector3 randomPos = transform.position;
        randomPos.y = UnityEngine.Random.Range(start.y - yMaxChange, start.y + yMaxChange);
        transform.position = randomPos;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        if(increasing)
        {
            pos.y += Time.deltaTime * speed;
            if(pos.y >= start.y + yMaxChange)
            {
                increasing = false;
            }
        } else
        {
            pos.y -= Time.deltaTime * speed;
            if (pos.y <= start.y - yMaxChange)
            {
                increasing = true;
            }
        }
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            OnPickup?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
