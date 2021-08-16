using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LevelManager : MonoBehaviour {

    public enum LEVELSTATE { WaitForBegin = 0, InitialConstruction = 1, SpawnWave = 2, PauseWave = 3, Complete = 4 };

    // Wave definition structure:
    // timeTillSpawn:unitID; ... timeTillSpawn:unitID; -1:-1
    // -1:-1 = end of wave
    // 0 = basic, 1 = dangerous, 2 = fast, 3 = boss

    private const string FIRSTWAVEDEFINITION = "1:0;1:0;1:0;1:0;1:0;"                                              // 5 basic
                                             + "1:0;1:0;1:2;1:2;1:0;1:0;1:2;1:2;"                                  // (2 basic, 2 fast) * 2
                                             + "1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:0;1:0;1:2;1:2;1:2;1:2;-1:-1";  // (4 basic, 4 fast) * 2

    private const string SECONDWAVEDEFINTION = "1:1;1:1;1:1;1:1;1:1;"                                              // 5 dangerous
                                             + "1:2;1:2;1:1;1:2;1:2;1:1;1:2;1:2;1:1;"                              // (2 fast, 1 dangerous) * 3
                                             + "1:0;1:1;1:2;1:0;1:1;1:2;1:0;1:1;1:2;-1:-1";                              // (1 basic, 1 dangerous, 1 fast) * 3

    private const string THIRDWAVEDEFINTION = "1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;1:2;"                   // 12 fast
                                             + "1:0;1:0;1:2;1:2;1:2;1:2;1:0;1:0;1:2;1:2;1:2;1:2;1:1;1:1;1:1;1:1;"  // (2 basic, 4 fast) * 2, 5 dangerous
                                             + "1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;5:3;-1:-1;";  // (2 basic, 2 dangerous, 2 fast) * 3, 1 boss

    // Boss, Dangerous, Boss, Dangerous, Boss, Boss, Dangerous, Boss, Dangerous, Boss
    private const string BOSSWAVEDEFINTION = "1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;1.5:3;1.5:1;1.5:3;1.5:1;1.5:3;5:4;-1:-1";        


    private string[] waveData = new string[]{ FIRSTWAVEDEFINITION, SECONDWAVEDEFINTION, THIRDWAVEDEFINTION, BOSSWAVEDEFINTION };  // Complete

    public float pauseWaveTime = 10;
    public float stateTimer;
    public int waveID = 0;

    private List<GameObject> towerSnapPoints;
    private List<GameObject> towers;

    private AISpawner spawner;
    private CameraBehaviour cameraRef;

    public LEVELSTATE levelState = LEVELSTATE.WaitForBegin;

    private bool loaded = false;
    public bool towerCreatePhase = false;
    
    public TowerStatusDialog dialogSelected;

    // Use this for initialization
    void Start () {
        towerSnapPoints = new List<GameObject>();
        towers = new List<GameObject>();
        spawner = null;
        cameraRef = Camera.main.GetComponent<CameraBehaviour>();
    }
	
	// Update is called once per frame
    public void update(float deltaTime)
    {
        if(!loaded) {
            firstUpdateInit();
            loaded = true;
        }
        
        foreach (GameObject tower in towers)
        {
            if (tower != null)
            {
                tower.GetComponent<TowerBehaviour>().update(deltaTime);
            }
        }

        spawner.update(deltaTime);
        dialogSelected.update(deltaTime);
        updateLevelState(deltaTime);
	}

    public void updateLevelState(float deltaTime)
    {
        switch(levelState)
        {
            case LEVELSTATE.SpawnWave:
                if (!spawner.spawning)
                {
                    nextState();
                }
                break;
            case LEVELSTATE.InitialConstruction:
            case LEVELSTATE.PauseWave:
                stateTimer -= deltaTime;
                if(stateTimer <= 0)
                {
                    nextState();
                }
                break;
        }
    }

    public void nextState()
    {
        if (levelState == LEVELSTATE.PauseWave && waveID < waveData.Length)
        {
            levelState--;
        }
        else
        {
            levelState++;
        }

        switch (levelState)
        {
            case LEVELSTATE.SpawnWave:
                spawner.spawnWave(waveData[waveID++]);
                break;
            case LEVELSTATE.InitialConstruction:
            case LEVELSTATE.PauseWave:
                stateTimer = pauseWaveTime;
                break;
        }
    }

    public void cancelTowerCreation()
    {
        if (!towerCreatePhase)
            return;
        destroySelectedTower();
        towerCreatePhase = false;
    }

    public void destroySelectedTower()
    {
        if (cameraRef.selectedTower != null)
        {
            TowerBehaviour script = cameraRef.selectedTower;
            script.destroyTower();
            foreach (GameObject snap in towerSnapPoints)
            {
                if (snap.GetComponent<TowerSnapBehaviour>().associatedTower == script.gameObject)
                {
                    snap.GetComponent<TowerSnapBehaviour>().associatedTower = null;
                }
            }
            towers.Remove(script.gameObject);
            cameraRef.forceDeselectImmediately();
            Destroy(script.gameObject);
        }
    }

    public void addTower(GameObject tower)
    {
        towers.Add(tower);
    }

    private void firstUpdateInit()
    {
        levelState = LEVELSTATE.WaitForBegin;
        GameObject[] snapPoints = GameObject.FindGameObjectsWithTag("TowerSnap");
        foreach(GameObject snapPoint in snapPoints) {
            towerSnapPoints.Add(snapPoint);
        }

        GameObject[] prespawnedTowers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in prespawnedTowers)
        {
            towers.Add(tower);
        }

        spawner = GameObject.FindGameObjectWithTag("AISpawner").GetComponent<AISpawner>();
        nextState();
    }
}
