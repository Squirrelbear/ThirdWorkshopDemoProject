using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DAISpawnerBehaviour : MonoBehaviour
{
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


    private string[] waveDefinitions = new string[] { FIRSTWAVEDEFINITION, SECONDWAVEDEFINTION, THIRDWAVEDEFINTION, BOSSWAVEDEFINTION };

    public GameObject[] prefabAI;
    public bool spawning = false;
    public GameObject firstNavNode;

    private Transform spawnAt;
    public bool spawningWave = false;
    public float timeBetweenWave = 10;

    public int majorWaveID;
    public int currentWave;
    public bool repeatWaves = true;
    public List<List<WaveCommand>> waveData;
    public int dataIndex;

    public int spawnedEnemyCount;
    public List<GameObject> waitingSpawnEnemies;

    float doNothingTillZero = 0;

    private string repeatWaveDefinition = "";

    // Use this for initialization
    void Start()
    {
        spawnAt = transform;
        spawning = false;
        setNextWaveGroup();
    }

    // Update is called once per frame
    void Update()
    {
        doNothingTillZero -= Time.deltaTime;
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
                    if (majorWaveID >= waveDefinitions.Length)
                    {
                        spawning = false;
                        spawningWave = false;
                    } else
                    {
                        setNextWaveGroup();
                    }
                }
            }
            else if (dataIndex >= waveData[currentWave].Count) // End of current wave
            {
                spawningWave = false;
                if (spawnedEnemyCount == 0 && waitingSpawnEnemies.Count == 0)
                {
                    currentWave++;
                    spawnWave();
                    doNothingTillZero = timeBetweenWave;
                }
            }
            else if (spawningWave)
            {
                executeCurrentWaveCommand();
            }
        }
    }

    private void HandleEnemyDestroyed()
    {
        spawnedEnemyCount--;
    }

    private void executeCurrentWaveCommand()
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
            waitingSpawnEnemies[0].GetComponent<DHealthBehaviour>().UnitDied += HandleEnemyDestroyed;
            waitingSpawnEnemies[0].GetComponent<DMoveToTargetBehaviour>().TargetReached += HandleEnemyDestroyed;
            waitingSpawnEnemies.RemoveAt(0);
        }
        dataIndex++;
    }

    public void setNextWaveGroup()
    {
        spawnWave(waveDefinitions[majorWaveID++]);
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
            spawnedEnemyCount++;
            var aiUnit = Instantiate(prefabAI[unitID], spawnAt.position, spawnAt.rotation);
            aiUnit.transform.parent = gameObject.transform;
            DMoveToTargetBehaviour ai = aiUnit.GetComponent<DMoveToTargetBehaviour>();
            ai.currentWaypoint = firstNavNode;
            return aiUnit;
        }
        return null;
    }

    private void spawnWave()
    {
        waitingSpawnEnemies = new List<GameObject>();

        if (currentWave >= waveData.Count)
        {
            return;
        }

        foreach (WaveCommand cmd in waveData[currentWave])
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
            foreach (string evnt in allEvents)
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



}
