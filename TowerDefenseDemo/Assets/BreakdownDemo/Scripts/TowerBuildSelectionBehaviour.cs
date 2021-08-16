using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuildSelectionBehaviour : MonoBehaviour
{
    public int towerSpawnID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        TowerChoiceManager.instance.spawnTowerByID(towerSpawnID);
    }
}
