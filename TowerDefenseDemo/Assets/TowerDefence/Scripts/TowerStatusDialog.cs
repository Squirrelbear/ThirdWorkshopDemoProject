using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerStatusDialog : MonoBehaviour {
    // cached references
    public Text tooltipTitleText;
    public Camera cam;
    public CameraBehaviour camRef;
    public TowerBehaviour cachedTowerRef;
    
	// Use this for initialization
	void Start () {
        cam = Camera.main;
        camRef = cam.GetComponent<CameraBehaviour>();
        hide();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {

        if (camRef.selectedTower == null || camRef.selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            hide();
            return;
        }
        else
        {
            show();
        }

        if (camRef.selectedTower != cachedTowerRef)
        {
            cachedTowerRef = camRef.selectedTower;
            updateTowerTitle();
        }
        
        // Face the camera
        transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);

        // Position 5 units above the tower
        transform.position = cachedTowerRef.transform.position + new Vector3(0, 5, 0);

	}

    private void updateTowerTitle()
    {
        if(cachedTowerRef != null)
        {
            switch(cachedTowerRef.towerType)
            {
                case TowerBehaviour.TowerType.Basic:
                    tooltipTitleText.text = "Basic Tower";
                    break;
                case TowerBehaviour.TowerType.Frost:
                    tooltipTitleText.text = "Frost Tower";
                    break;
                case TowerBehaviour.TowerType.Swarm:
                    tooltipTitleText.text = "Swarm Tower";
                    break;
                case TowerBehaviour.TowerType.Explosive:
                    tooltipTitleText.text = "Explosive Tower";
                    break;
            }
        }
    }

    public void hide()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    public void show()
    {
        gameObject.GetComponent<Canvas>().enabled = true;
    }
}
