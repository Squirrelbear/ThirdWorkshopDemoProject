using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerChoiceManager : MonoBehaviour
{
    public List<GameObject> towerPrefabs;
    public DTowerSnapPointBehaviour currentSnapPoint;
    public static TowerChoiceManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void callToSnapPoint(DTowerSnapPointBehaviour snapPointReference)
    {
        instance.currentSnapPoint = snapPointReference;
        Vector3 position = snapPointReference.snapPoint;
        position.x -= 3f;
        instance.gameObject.transform.position = position;
        instance.gameObject.SetActive(true);
    }

    public static void hide()
    {
        instance.gameObject.SetActive(false);
    }

    public void spawnTowerByID(int towerID)
    {
        if(towerID >= 0 && towerID < towerPrefabs.Count)
        {
            GameObject objRef = Instantiate(towerPrefabs[towerID], currentSnapPoint.snapPoint, Quaternion.identity);
            currentSnapPoint.associatedTower = objRef;
        }
    }
}
