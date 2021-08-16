using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    public enum CameraState { Neutral, PlacingTower, ControllingTower };
    
    public float timeToSelect = 1.5f;
    public float deselectionProgress = 0;

    public TowerBehaviour selectedTower;
    public GameObject selectedObject;

    public TowerSnapBehaviour[] towerSnapPoints;
    public bool isTowerSnapped = false;
    public TowerSnapBehaviour tempSnapCache = null;
    public CameraState cameraState = CameraState.Neutral;
    

    private Ray ray;

    public GameObject[] towerPrefabs;
    
    public LevelManager levelManager = null;

    public bool useTerrainSnap = true;
    private bool firstUpdate = true;

    // Mouse camera control
    public float rotationX, rotationY;
    public Vector3 position;
    //public float minY = -45.0f;
    //public float maxY = 45.0f;
    public float sensX = 100.0f;
    public float sensY = 100.0f;
    public float camMinPosX, camMinPosZ, camMaxPosX, camMaxPosZ;

    private bool pause = false;

	// Use this for initialization
	void Start () {
        GameObject[] towerSnapObjs = GameObject.FindGameObjectsWithTag("TowerSnap");
        towerSnapPoints = new TowerSnapBehaviour[towerSnapObjs.Length];
        for(int i = 0; i < towerSnapObjs.Length; i++) 
        {
            towerSnapPoints[i] = towerSnapObjs[i].GetComponent<TowerSnapBehaviour>();
            towerSnapObjs[i].GetComponent<MeshRenderer>().enabled = false;
        }

        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
	}

    // Update is called once per frame
    void Update()
    {
        if(firstUpdate)
        {
            rotationX = transform.localEulerAngles.x;
            rotationY = transform.localEulerAngles.y;
            position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            firstUpdate = false;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            pause = !pause;
        }
        if(pause)
        {
            return;
        }
        
        // Master definition of deltaTime for all objects
        float deltaTime = Time.deltaTime;

        this.ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        updateCameraPosition(deltaTime);
        updateTowerDefence(deltaTime);
	}

    private bool attemptSelection(float deltaTime)
    {
        if(selectedObject != null && selectedTower != null  && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            return false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            GameObject tempObj = findObjectByTagInRay(ray, "Tower");
            if (tempObj != null)
            {
                selectTower(tempObj);
                isTowerSnapped = false;
            }
        }

        return true;
    }

    private void updateCameraPosition(float deltaTime)
    {
        if (Input.GetMouseButton(1))
        {
            rotationX -= Input.GetAxis("Mouse X") * sensX * deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * sensY * deltaTime;
            //rotationY = Mathf.Clamp(rotationY, minY, maxY);
            transform.localEulerAngles = new Vector3(-rotationY, -rotationX, 0);
        }
        if (Input.GetMouseButton(2))
        {
            position.x += Input.GetAxis("Mouse Y") * sensX * deltaTime;
            position.z -= Input.GetAxis("Mouse X") * sensX * deltaTime;
            position.x = Mathf.Clamp(position.x, camMinPosX, camMaxPosX);
            position.z = Mathf.Clamp(position.z, camMinPosZ, camMaxPosZ);
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            position.x -= sensX * deltaTime;
            position.x = Mathf.Clamp(position.x, camMinPosX, camMaxPosX);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            position.x += sensX * deltaTime;
            position.x = Mathf.Clamp(position.x, camMinPosX, camMaxPosX);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            position.z -= sensY * deltaTime;
            position.z = Mathf.Clamp(position.z, camMinPosZ, camMaxPosZ);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            position.z += sensY * deltaTime;
            position.z = Mathf.Clamp(position.z, camMinPosZ, camMaxPosZ);
        }
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            // Hard coded camera reset
            position = new Vector3(81.7f, 39.41f, -29.2f);
            rotationX = 90;
            rotationY = 270;
            transform.localEulerAngles = new Vector3(-rotationY, -rotationX, 0);
        }

        transform.position = position;
    }
    
    private GameObject findObjectByTagInRay(Ray ray, string tag)
    {
        RaycastHit[] hit;
        hit = Physics.RaycastAll(ray);
        if (hit.Length == 0)
            return null;

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].transform.tag.Equals(tag))
            {
                return hit[i].transform.gameObject;
            }
        }
        return null;
    }

    public void selectTower(GameObject tower)
    {
        selectedObject = tower;

        if (tower != null)
        {
            selectedTower = tower.GetComponent<TowerBehaviour>();
        }
    }

    public void deSelectTower()
    {
        tempSnapCache.associatedTower = selectedTower.gameObject;
        if(selectedTower != null)
        {
            selectedTower.setTowerState(TowerBehaviour.TowerState.TowerAIControlled);
        }

        selectedObject = null;
        selectedTower = null;

        foreach (TowerSnapBehaviour snapPoint in towerSnapPoints)
        {
            snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        levelManager.towerCreatePhase = false;
    }

    public void forceDeselectImmediately()
    {
        selectedObject = null;
        selectedTower = null;
    }

    public bool attemptDeSelectTower(float deltaTime)
    {
        if (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            if(isTowerSnapped)
            {
                if (deselectionProgress >= timeToSelect)
                {
                    deSelectTower();
                    return true;
                }
                else
                {
                    deselectionProgress += deltaTime;
                    return false;
                }
            }
            else
            {
                deselectionProgress = 0;
            }
        }
        deselectionProgress = 0;
        return false;
    }

    private void updateSelectedTower()
    {
        if (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            if (useTerrainSnap)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 9999.0f, 1 << 8))
                {
                    Vector3 newPoint = ray.GetPoint(hit.distance);
                    newPoint.y += 1.59f;

                    float distanceSnap = 99999;
                    Vector3 snapLocation = newPoint;
                    isTowerSnapped = false;
                    foreach (TowerSnapBehaviour towerSnapPoint in towerSnapPoints)
                    {
                        if (!towerSnapPoint.isAvailable()) { continue; }

                        float distanceTemp = Vector3.Distance(towerSnapPoint.snapPoint, newPoint);
                        if (distanceTemp < distanceSnap && distanceTemp < 5)
                        {
                            snapLocation = towerSnapPoint.snapPoint;
                            distanceSnap = distanceTemp;
                            isTowerSnapped = true;
                            tempSnapCache = towerSnapPoint;
                            break;
                        }
                    }

                    selectedObject.transform.position = snapLocation;
                }
            }
            else
            {
                // create a plane at 0,0,0 whose normal points to +Y:
                Plane hPlane = new Plane(Vector3.up, new Vector3(0, 0, 0));
                // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
                float distance = 0;
                // if the ray hits the plane...
                if (hPlane.Raycast(ray, out distance))
                {
                    // get the hit point:
                    Vector3 newPoint = ray.GetPoint(distance);

                    float distanceSnap = 99999;
                    Vector3 snapLocation = newPoint;
                    isTowerSnapped = false;
                    foreach (TowerSnapBehaviour towerSnapPoint in towerSnapPoints)
                    {
                        float distanceTemp = Vector3.Distance(towerSnapPoint.snapPoint, newPoint);
                        if (distanceTemp < distanceSnap && distanceTemp < 1)
                        {
                            snapLocation = towerSnapPoint.snapPoint;
                            distanceSnap = distanceTemp;
                            isTowerSnapped = true;
                        }
                    }

                    selectedObject.transform.position = snapLocation;
                }
            }
        }
    }

    public TowerBehaviour getSelectedTower()
    {
        return selectedTower;
    }

    private bool keyboardAction()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            moveSelectedTower();
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            levelManager.destroySelectedTower();
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            spawnTower(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            spawnTower(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            spawnTower(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            spawnTower(3);
        }
        /*else if(Input.GetKeyDown(KeyCode.Escape))
        {
            levelManager.cancelTowerCreation();
        }*/

        return false;
    }

    public void spawnTower(int towerID)
    {
        if (towerID < 0 || towerID >= towerPrefabs.Length || (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement))
        {
            return;
        }
        GameObject tower = (GameObject)Instantiate(towerPrefabs[towerID], new Vector3(0, 1.59f, 0), Quaternion.identity);
        levelManager.addTower(tower);
        selectTower(tower);
        moveSelectedTower();
    }

    public void moveSelectedTower()
    {
        if (selectedTower != null)
        {
            isTowerSnapped = false;
            deselectionProgress = 0;
            foreach(TowerSnapBehaviour towerSnap in towerSnapPoints)
            {
                if (towerSnap.associatedTower == selectedTower.gameObject)
                {
                    towerSnap.associatedTower = null;
                }
            }
            
            selectedTower.setTowerState(TowerBehaviour.TowerState.TowerPlacement);
            foreach (TowerSnapBehaviour snapPoint in towerSnapPoints)
            {
                snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void updateTowerDefence(float deltaTime)
    {
        if (keyboardAction())
        {
            // do nothing
        }
        else if (attemptSelection(deltaTime))
        {
            // do nothing
        }
        else if (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement
            && isTowerSnapped && Input.GetMouseButtonDown(0))
        {
            // Debug only deselection. Places tower back down on a snap with a click
            deSelectTower();
        }
        else if(attemptDeSelectTower(deltaTime)) // Drop tower by holding at a snap point
        {
            // do nothing
        }
        else
        {
            updateSelectedTower();
        }


        levelManager.update(Time.deltaTime);
    }
}
