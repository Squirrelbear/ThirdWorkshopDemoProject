using UnityEngine;
using System.Collections;

public class SpawnAndDestroyEffect : MonoBehaviour {

    public float timeCreate = 1;
    public float timeDestroy = 1;
    public GameObject prefab;
    private float timer;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeCreate && timeCreate != -1)
        {
            var newObj = Instantiate(prefab, transform.position, transform.rotation);
            newObj.transform.parent = gameObject.transform.parent.transform;
            timeCreate = -1;
        }
        if (timer > timeDestroy)
        {
            Destroy(gameObject);
        }
    }
}
