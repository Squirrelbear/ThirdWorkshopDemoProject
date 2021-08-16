using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISpawner : MonoBehaviour {

    public GameObject[] prefabAI;
    public bool spawning = false;
    public GameObject firstNavNode;

    private Transform spawnAt;
    public bool spawningWave = false;
    public float timeBetweenWave = 10;
    
    public int currentWave;
    public bool repeatWaves = true;
    public List<List<WaveCommand>> waveData;
    public int dataIndex;

    public List<GameObject> currentWaveEnemies;
    public List<GameObject> waitingSpawnEnemies;
    
    float doNothingTillZero = 0;

    private string repeatWaveDefinition = "";

    public LevelManager levelManager; 

	// Use this for initialization
	void Start () {
        spawnAt = transform;
        spawning = false;
	}

	// Update is called once per frame
    public void update(float deltaTime)
    {
        // Update all currently living enemy units
        foreach (GameObject enemy in currentWaveEnemies)
        {
            if (enemy != null)
            {
                enemy.GetComponent<AIWayfinder>().update(deltaTime);
            }
        }

        doNothingTillZero -= deltaTime;
        if (doNothingTillZero > 0) return;
        doNothingTillZero = 0;

        if (spawning)
        {
            if (currentWave >= waveData.Count) // End of all waves
            {
                if (repeatWaves)
                {
                    parseWaveData(repeatWaveDefinition);
                    currentWave = 0;
                    spawningWave = false;
                    spawnWave();
                }
                else
                {
                    spawning = false;
                    spawningWave = false;
                }
            }
            else if (dataIndex >= waveData[currentWave].Count) // End of current wave
            {
                spawningWave = false;
                for (int i = 0; i < currentWaveEnemies.Count; i++)
                {
                    if (currentWaveEnemies[i] == null)
                    {
                        currentWaveEnemies.RemoveAt(i);
                    }
                }
                if (currentWaveEnemies.Count == 0 && waitingSpawnEnemies.Count == 0)
                {
                    currentWave++;
                    spawnWave();
                    doNothingTillZero = timeBetweenWave;
                }
            }
            else if (spawningWave)
            {
                if (waveData[currentWave][dataIndex].timeToWait > 0)
                {
                    doNothingTillZero = waveData[currentWave][dataIndex].timeToWait;
                    waveData[currentWave][dataIndex].timeToWait = 0;
                    return;
                }

                if (waveData[currentWave][dataIndex].unitIDToSpawn >= 0 && waveData[currentWave][dataIndex].unitIDToSpawn < prefabAI.Length)
                {
                    waitingSpawnEnemies[0].SetActive(true);
                    currentWaveEnemies.Add(waitingSpawnEnemies[0]);
                    if (waitingSpawnEnemies[0].GetComponent<AIWayfinder>().unitType == AIWayfinder.UnitType.MegaBoss)
                    {
                        GameObject.Find("ExplosionRocks").GetComponent<ExplosionSimulationManager>().explode();
                    }
                    waitingSpawnEnemies.RemoveAt(0);
                }
                dataIndex++;
            }
        }
	}

    public void spawnWave(string waveDefinition)
    {
        parseWaveData(waveDefinition);
        spawnWave();
        spawning = true;
    }

    private GameObject spawnUnit(int unitID)
    {
        if (prefabAI.Length > unitID && unitID >= 0)
        {
            var aiUnit = Instantiate(prefabAI[unitID], spawnAt.position, spawnAt.rotation);
            aiUnit.transform.parent = gameObject.transform;
            AIWayfinder ai = aiUnit.GetComponent<AIWayfinder>();
            ai.wayPoint = firstNavNode;
            return aiUnit;
        }
        return null;
    }

    private void spawnWave()
    {
        currentWaveEnemies = new List<GameObject>();
        waitingSpawnEnemies = new List<GameObject>();

        if (currentWave >= waveData.Count)
        {
            return;
        }

        foreach(WaveCommand cmd in waveData[currentWave])
        {
            GameObject newUnit = spawnUnit(cmd.unitIDToSpawn);
            if (newUnit != null)
            {
                newUnit.SetActive(false);
                waitingSpawnEnemies.Add(newUnit);
            }
        }

        spawningWave = true;
        dataIndex = 0;
    }

    private void parseWaveData(string waveDefinition)
    {
        repeatWaveDefinition = waveDefinition;
        waveData = new List<List<WaveCommand>>();
        currentWave = 0;
        if (waveDefinition.Length > 0)
        {
            string[] allEvents = waveDefinition.Split(';');
            List<WaveCommand> curWave = new List<WaveCommand>();
            foreach(string evnt in allEvents)
            {
                WaveCommand curEvent = WaveCommand.getWaveCommand(evnt);
                if (curEvent.timeToWait == -1 && curEvent.unitIDToSpawn == -1)
                {
                    waveData.Add(curWave);
                    curWave = new List<WaveCommand>();
                }
                else
                {
                    curWave.Add(curEvent);
                }
            }
            if (curWave.Count > 0)
            {
                waveData.Add(curWave);
            }
        }
    }

    // Original co-routine version. Do not use.
    private IEnumerator beginSpawning()
    {
        while (spawning)
        {
            if (currentWave >= waveData.Count) // End of all waves
            {
                print("End of Wave Detected");
                if (repeatWaves)
                {
                    currentWave = 0;
                    spawningWave = false;
                    spawnWave();
                }
                else
                {
                    spawning = false;
                    spawningWave = false;
                }
            }
            else if (dataIndex >= waveData[currentWave].Count) // End of current wave
            {
                spawningWave = false;
                for (int i = 0; i < currentWaveEnemies.Count; i++)
                {
                    if (currentWaveEnemies[i] == null)
                    {
                        currentWaveEnemies.RemoveAt(i);
                    }
                }
                if (currentWaveEnemies.Count == 0 && waitingSpawnEnemies.Count == 0)
                {
                    currentWave++;
                    spawnWave();
                }
            }
            else if (spawningWave)
            {
                if (waveData[currentWave][dataIndex].timeToWait > 0)
                {
                    yield return new WaitForSeconds(waveData[currentWave][dataIndex].timeToWait);
                }

                if (waveData[currentWave][dataIndex].unitIDToSpawn >= 0 && waveData[currentWave][dataIndex].unitIDToSpawn < prefabAI.Length)
                {
                    waitingSpawnEnemies[0].SetActive(true);
                    currentWaveEnemies.Add(waitingSpawnEnemies[0]);
                    waitingSpawnEnemies.RemoveAt(0);
                }
                dataIndex++;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
